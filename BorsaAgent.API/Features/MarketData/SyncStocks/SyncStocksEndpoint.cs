using BorsaAgent.API.Common;

namespace BorsaAgent.API.Features.MarketData.SyncStocks;

public static class SyncStocksEndpoint
{
    public static void MapSyncStocksEndpoint(this WebApplication app)
    {
        app.MapPost("/api/market-data/sync-stocks", Handle)
            .WithName("SyncStocks")
            .WithOpenApi()
            .WithDescription("BIST hisselerini TwelveData'dan çekip veritabanına kaydet")
            .Produces<Result<SyncStocksResponse>>(StatusCodes.Status200OK)
            .Produces<Result<SyncStocksResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(SyncStocksHandler handler)
    {
        var result = await handler.Handle(CancellationToken.None);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
}