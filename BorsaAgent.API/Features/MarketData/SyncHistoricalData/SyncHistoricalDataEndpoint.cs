using BorsaAgent.API.Common;

namespace BorsaAgent.API.Features.MarketData.SyncHistoricalData;

public static class SyncHistoricalDataEndpoint
{
    public static void MapSyncHistoricalDataEndpoint(this WebApplication app)
    {
        app.MapPost("/api/market-data/sync-historical-data", Handle)
            .WithName("SyncHistoricalData")
            .WithOpenApi()
            .WithDescription("İş Yatırım'dan tarihsel veri çekip veritabanına kaydet (01.01.2024 - Bugün)")
            .Produces<Result<SyncHistoricalDataResponse>>(StatusCodes.Status200OK)
            .Produces<Result<SyncHistoricalDataResponse>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(SyncHistoricalDataHandler handler)
    {
        var result = await handler.Handle(CancellationToken.None);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
}