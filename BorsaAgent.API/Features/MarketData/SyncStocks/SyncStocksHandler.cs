using BorsaAgent.API.Common;
using BorsaAgent.API.Infrastructure.Database;
using BorsaAgent.API.Infrastructure.Database.Models;
using BorsaAgent.API.Infrastructure.TwelveDataClient;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Features.MarketData.SyncStocks;

public class SyncStocksHandler
{
    private readonly BorsaAgentDbContext _dbContext;
    private readonly ITwelveDataApiClient _twelveDataClient;
    private readonly ILogger<SyncStocksHandler> _logger;

    public SyncStocksHandler(
        BorsaAgentDbContext dbContext,
        ITwelveDataApiClient twelveDataClient,
        ILogger<SyncStocksHandler> logger)
    {
        _dbContext = dbContext;
        _twelveDataClient = twelveDataClient;
        _logger = logger;
    }

    public async Task<Result<SyncStocksResponse>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("BIST hisseleri senkronizasyonu başladı");

            // TwelveData'dan hisseleri çek
            List<StockInfo> bistStocks = await _twelveDataClient.GetBistStocksAsync(cancellationToken);

            if (bistStocks.Count == 0)
            {
                return Result<SyncStocksResponse>.Failure("TwelveData'dan hisse çekilemedi");
            }

            var newStocks = new List<Stock>();
            var updatedStocks = new List<Stock>();

            foreach (var stockInfo in bistStocks)
            {
                var existingStock = await _dbContext.Stocks
                    .FirstOrDefaultAsync(s => s.Symbol == stockInfo.Symbol, cancellationToken);

                if (existingStock == null)
                {
                    newStocks.Add(new Stock
                    {
                        Symbol = stockInfo.Symbol,
                        Name = stockInfo.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else if (existingStock.Name != stockInfo.Name)
                {
                    existingStock.Name = stockInfo.Name;
                    existingStock.UpdatedAt = DateTime.UtcNow;
                    updatedStocks.Add(existingStock);
                }
            }

            if (newStocks.Count > 0)
            {
                await _dbContext.Stocks.AddRangeAsync(newStocks, cancellationToken);
            }

            if (updatedStocks.Count > 0)
            {
                _dbContext.Stocks.UpdateRange(updatedStocks);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var totalStocks = await _dbContext.Stocks.CountAsync(cancellationToken);

            _logger.LogInformation("BIST senkronizasyonu tamamlandı. Toplam: {Total}, Yeni: {New}, Güncellenen: {Updated}",
                totalStocks, newStocks.Count, updatedStocks.Count);

            return Result<SyncStocksResponse>.Success(new SyncStocksResponse
            {
                TotalStocks = totalStocks,
                NewStocksAdded = newStocks.Count,
                StocksUpdated = updatedStocks.Count,
                Message = $"Senkronizasyon başarılı. {newStocks.Count} yeni, {updatedStocks.Count} güncellenen hisse."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BIST senkronizasyonu sırasında hata oluştu");
            return Result<SyncStocksResponse>.Failure($"Hata: {ex.Message}");
        }
    }
}

public class SyncStocksResponse
{
    public int TotalStocks { get; set; }
    public int NewStocksAdded { get; set; }
    public int StocksUpdated { get; set; }
    public string Message { get; set; } = string.Empty;
}