using BorsaAgent.API.Data;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace BorsaAgent.API.Features.DataCollector;

public class YahooFinanceService(IHttpClientFactory httpClientFactory, IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Yahoo");

    private static readonly DateTimeOffset Period1Utc = new(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private const string Interval = "1d";
    private const int MaxConcurrentRequests = 5;
    private static readonly SemaphoreSlim Semaphore = new(MaxConcurrentRequests, MaxConcurrentRequests);

    public async Task<int> SyncAllStockPricesAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var stocks = await db.Stocks
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new Stock { Id = s.Id, Code = s.Code })
            .ToListAsync(ct);

        int total = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5, // aynı anda max 5 hisse
            CancellationToken = ct      // ✅ cancel direkt buraya bağlı
        };

        await Parallel.ForEachAsync(stocks, options, async (stock, ct) =>
        {
            var inserted = await SyncOneStockAsync(stock, ct);
            Interlocked.Add(ref total, inserted); // thread-safe toplama
        });

        return total;
    }

    private async Task<int> SyncOneStockAsync(Stock stock, CancellationToken ct)
    {
        await Semaphore.WaitAsync(ct);
        try
        {
            await using var db1 = await dbFactory.CreateDbContextAsync(ct);

            var lastDate = await db1.DailyPrices
                .AsNoTracking()
                .Where(x => x.StockId == stock.Id)
                .MaxAsync(x => (DateOnly?)x.TradeDate);

            var period2Utc = DateTimeOffset.UtcNow;

            var period1 = lastDate.HasValue
                ? new DateTimeOffset(lastDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds()
                : Period1Utc.ToUnixTimeSeconds();

            var period2 = period2Utc.ToUnixTimeSeconds();

            var url = $"chart/{stock.Code}?interval={Interval}&period1={period1}&period2={period2}";

            YahooFinanceResponse? response;
            try
            {
                response = await _httpClient.GetFromJsonAsync<YahooFinanceResponse>(url, ct);
            }
            catch (OperationCanceledException)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "Cancelled by request", CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, ex.Message, CancellationToken.None);
                return 0;
            }

            if (response?.Chart?.Error != null)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, response.Chart.Error.Description, CancellationToken.None);
                return 0;
            }

            var result = response?.Chart?.Result?.FirstOrDefault();
            var quote = result?.Indicators?.Quote?.FirstOrDefault();

            if (result?.Timestamp == null || quote == null)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "Empty result", CancellationToken.None);
                return 0;
            }

            var count = new[]
            {
                result.Timestamp.Count,
                quote.Open?.Count ?? 0,
                quote.High?.Count ?? 0,
                quote.Low?.Count ?? 0,
                quote.Close?.Count ?? 0,
                quote.Volume?.Count ?? 0
            }.Min();

            if (count == 0)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "No data points", CancellationToken.None);
                return 0;
            }

            // Null gelen satırları atlayalım
            var rows = new List<(DateOnly d, decimal o, decimal h, decimal l, decimal c, long v, decimal? changePercent)>(capacity: count);
            for (int i = 0; i < count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var o = quote.Open![i];
                var h = quote.High![i];
                var l = quote.Low![i];
                var c = quote.Close![i];
                var v = quote.Volume![i];

                if (o is null || h is null || l is null || c is null || v is null)
                    continue;

                var tradeDate = DateOnly.FromDateTime(
                    DateTimeOffset.FromUnixTimeSeconds(result.Timestamp[i]).UtcDateTime);

                rows.Add((tradeDate, o.Value, h.Value, l.Value, c.Value, v.Value, null));
            }

            if (rows.Count == 0)
            {
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "All rows contained nulls", CancellationToken.None);
                return 0;
            }

            // Sıralayıp ChangePercent hesapla
            rows = rows.OrderBy(r => r.d).ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                if (i == 0) continue;

                var prev = rows[i - 1].c;
                var curr = rows[i].c;

                var changePercent = prev != 0
                    ? Math.Round((curr - prev) / prev * 100, 2)
                    : (decimal?)null;

                rows[i] = rows[i] with { changePercent = changePercent };
            }

            // DB insert (batch + on conflict)
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var conn = (NpgsqlConnection)db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync(ct);

                const string sql = @"INSERT INTO ""DailyPrice""
                                   (""StockId"", ""TradeDate"", ""OpenPrice"", ""HighPrice"", ""LowPrice"", ""ClosePrice"", ""Volume"", ""ChangePercent"", ""CreatedAt"")
                                   VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
                                   ON CONFLICT (""StockId"", ""TradeDate"") DO NOTHING;";

                var createdAtUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                var batch = new NpgsqlBatch(conn, (NpgsqlTransaction)tx.GetDbTransaction()!);

                foreach (var r in rows)
                {
                    ct.ThrowIfCancellationRequested();

                    var cmd = new NpgsqlBatchCommand(sql);
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = stock.Id });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Date, Value = r.d.ToDateTime(TimeOnly.MinValue) });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, Value = r.o });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, Value = r.h });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, Value = r.l });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, Value = r.c });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Bigint, Value = r.v });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, Value = (object?)r.changePercent ?? DBNull.Value });
                    cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz, Value = createdAtUtc });

                    batch.BatchCommands.Add(cmd);
                }

                var affectedTotal = await batch.ExecuteNonQueryAsync(ct);

                await tx.CommitAsync(ct);

                // affectedTotal: eklenen + conflict nedeniyle 0 dönenlerin toplamı değil, sadece insert olanların sayısı olur
                //await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "affectedTotal:" + affectedTotal, CancellationToken.None);

                return affectedTotal;
            }
            catch (OperationCanceledException)
            {
                await tx.RollbackAsync(CancellationToken.None);
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, "Cancelled by request", CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(CancellationToken.None);
                await WriteLogAsync(stock.Id, url, Period1Utc, period2Utc, ex.Message, CancellationToken.None);
                return 0;
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task WriteLogAsync(int stockId,
                                     string requestUrl,
                                     DateTimeOffset p1,
                                     DateTimeOffset p2,
                                     string errorMessage,
                                     CancellationToken ct)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            db.StockSyncLogs.Add(new StockSyncLog
            {
                StockId = stockId,
                RequestUrl = requestUrl,
                Period1Utc = p1,
                Period2Utc = p2,
                ErrorMessage = errorMessage,
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Log yazılamadıysa ana akışı bozmayalım
        }
    }
}