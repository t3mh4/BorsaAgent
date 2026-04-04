using BorsaAgent.API.Features.StockImporter;

namespace BorsaAgent.API.Features.DataCollector;

public static class CollectorEndpoints
{
    public static void MapCollectorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/collector")
                       .WithTags("DataCollector");

        app.MapPost("/stocks/import", async (StockImporterService service, HttpContext ctx) =>
        {
            var (added, updated, skipped) = await service.ImportBistStocksAsync(ctx.RequestAborted);
            return Results.Ok(new { added, updated, skipped });
        });

        group.MapPost("/sync-prices", async (YahooFinanceService svc, HttpContext ctx) =>
        {
            var count = await svc.SyncAllStockPricesAsync(ctx.RequestAborted);
            return Results.Ok(new { Message = $"{count} yeni fiyat kaydı eklendi." });
        });
    }
}