using BorsaAgent.API.Data;
using BorsaAgent.API.Features.DataCollector;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BorsaAgent.API.Features.StockImporter;

public class StockImporterService(
    IHttpClientFactory httpClientFactory,
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<StockImporterService> logger)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("TwelveData");

    public async Task<(int added, int updated, int skipped)> ImportBistStocksAsync(CancellationToken ct = default)
    {
        TwelveDataResponse? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TwelveDataResponse>(
                "stocks?exchange=BIST", ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TwelveData'dan hisse listesi çekilemedi.");
            return (0, 0, 0);
        }

        if (response?.Data == null || response.Data.Count == 0)
        {
            logger.LogError("TwelveData boş response döndü.");
            return (0, 0, 0);
        }

        // Sadece Common Stock al
        var stocks = response.Data
            .Where(s => s.Type == "Common Stock" && !string.IsNullOrWhiteSpace(s.Symbol))
            .ToList();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var existingCodes = await db.Stocks
            .AsNoTracking()
            .Select(s => s.Code)
            .ToHashSetAsync(ct);

        int added = 0, updated = 0, skipped = 0;

        foreach (var s in stocks)
        {
            ct.ThrowIfCancellationRequested();

            // Yahoo Finance için .IS suffix ekle
            var yahooCode = $"{s.Symbol}.IS";

            if (existingCodes.Contains(yahooCode))
            {
                // Zaten var → name güncelle
                var existing = await db.Stocks.FirstAsync(x => x.Code == yahooCode, ct);
                if (existing.Name != s.Name)
                {
                    existing.Name = s.Name;
                    updated++;
                }
                else
                {
                    skipped++;
                }
                continue;
            }

            db.Stocks.Add(new Stock
            {
                Code = yahooCode,
                ShortCode = s.Symbol,
                Name = s.Name,
                IsActive = true
            });
            added++;
        }

        await db.SaveChangesAsync(ct);

        return (added, updated, skipped);
    }
}