using BorsaAgent.API.Features.StockImporter;

namespace BorsaAgent.API.Features.DataCollector;

public static class CollectorEndpoints
{
    public static void MapCollectorEndpoints(this WebApplication app)
    {
        var collectorGroup = app.MapGroup("/collector").WithTags("DataCollector");

        collectorGroup.MapPost("/stocks-import", async (StockImporterService service, HttpContext ctx) =>
        {
            var (added, updated, skipped) = await service.ImportBistStocksAsync(ctx.RequestAborted);
            return Results.Ok(new { added, updated, skipped });
        });

        collectorGroup.MapPost("/sync-prices", async (YahooFinanceService svc, HttpContext ctx) =>
        {
            var count = await svc.SyncAllStockPricesAsync(ctx.RequestAborted);
            return Results.Ok(new { Message = $"{count} yeni fiyat kaydı eklendi." });
        });
    }
}