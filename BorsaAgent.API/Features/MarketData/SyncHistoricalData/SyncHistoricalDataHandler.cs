using BorsaAgent.API.Common;
using BorsaAgent.API.Infrastructure.Database;
using BorsaAgent.API.Infrastructure.Database.Models;
using BorsaAgent.API.Infrastructure.IsYatirim;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Features.MarketData.SyncHistoricalData;

public class SyncHistoricalDataHandler
{
    private readonly IDbContextFactory<BorsaAgentDbContext> _dbContextFactory;
    private readonly IIsYatirimClient _isYatirimClient;
    private readonly ILogger<SyncHistoricalDataHandler> _logger;

    public SyncHistoricalDataHandler(
        IDbContextFactory<BorsaAgentDbContext> dbContextFactory,
        IIsYatirimClient isYatirimClient,
        ILogger<SyncHistoricalDataHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _isYatirimClient = isYatirimClient;
        _logger = logger;
    }

    public async Task<Result<SyncHistoricalDataResponse>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tarihsel veri senkronizasyonu başladı");

            // Ana context'ten hisseleri ve veri durumunu kontrol et
            using var mainContext = _dbContextFactory.CreateDbContext();

            var stocks = await mainContext.Stocks.ToListAsync(cancellationToken);

            if (stocks.Count == 0)
            {
                return Result<SyncHistoricalDataResponse>.Failure("Veritabanında hisse bulunamadı");
            }

            var hasExistingData = await mainContext.StockDailyData.AnyAsync(cancellationToken);

            DateTime startDate;
            DateTime endDate = DateTime.UtcNow;

            if (!hasExistingData)
            {
                startDate = new DateTime(2024, 1, 1);
                _logger.LogInformation("İlk senkronizasyon: {StartDate} - {EndDate} aralığında veri çekilecek",
                    startDate.Date, endDate.Date);
            }
            else
            {
                startDate = endDate.Date;
                _logger.LogInformation("Günlük senkronizasyon: {Date} için veri çekilecek", startDate.Date);
            }

            int totalRecordsAdded = 0;
            int totalRecordsSkipped = 0;
            var lockObject = new object();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(stocks, parallelOptions, async (stock, ct) =>
            {
                try
                {
                    _logger.LogInformation("İşleniyor: {Symbol}", stock.Symbol);

                    var historicalData = await _isYatirimClient.GetHistoricalDataAsync(stock.Symbol, startDate, endDate, ct);
                    if (historicalData.Count == 0)
                    {
                        _logger.LogWarning("{Symbol} için veri bulunamadı", stock.Symbol);
                        return;
                    }

                    // Her thread'in kendi DbContext'i var
                    using var context = _dbContextFactory.CreateDbContext();

                    var existingDates = await context.StockDailyData
                        .Where(d => d.StockId == stock.Id)
                        .Select(d => d.Date.Date)
                        .ToListAsync(ct);

                    var newRecords = new List<StockDailyData>();

                    foreach (var data in historicalData)
                    {
                        if (!DateTime.TryParseExact(data.HgdgTarih, "dd-MM-yyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var parsedDate))
                        {
                            _logger.LogWarning("Tarih parse edilemedi: {Date}", data.HgdgTarih);
                            continue;
                        }
                        var utcDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        if (!existingDates.Contains(parsedDate.Date))
                        {
                            newRecords.Add(new StockDailyData
                            {
                                StockId = stock.Id,
                                Date = utcDate,
                                Open = data.HgdgAof,
                                High = data.HgdgMax,
                                Low = data.HgdgMin,
                                Close = data.HgdgKapanis,
                                Volume = data.HgdgHacim,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    if (newRecords.Count > 0)
                    {
                        await context.StockDailyData.AddRangeAsync(newRecords, ct);
                        await context.SaveChangesAsync(ct);

                        lock (lockObject)
                        {
                            totalRecordsAdded += newRecords.Count;
                        }

                        _logger.LogInformation("{Symbol} için {Count} yeni kayıt eklendi",
                            stock.Symbol, newRecords.Count);
                    }
                    else
                    {
                        lock (lockObject)
                        {
                            totalRecordsSkipped += historicalData.Count;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hisse işlenirken hata oluştu: {Symbol}", stock.Symbol);
                }
            });

            _logger.LogInformation("Tarihsel veri senkronizasyonu tamamlandı. Eklenen: {Added}, Atlanan: {Skipped}",
                totalRecordsAdded, totalRecordsSkipped);

            return Result<SyncHistoricalDataResponse>.Success(new SyncHistoricalDataResponse
            {
                TotalRecordsAdded = totalRecordsAdded,
                TotalRecordsSkipped = totalRecordsSkipped,
                Message = $"Senkronizasyon başarılı. {totalRecordsAdded} yeni kayıt eklendi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarihsel veri senkronizasyonu sırasında hata oluştu");
            return Result<SyncHistoricalDataResponse>.Failure($"Hata: {ex.Message}");
        }
    }
}

public class SyncHistoricalDataResponse
{
    public int TotalRecordsAdded { get; set; }
    public int TotalRecordsSkipped { get; set; }
    public string Message { get; set; } = string.Empty;
}