using BorsaAgent.API.Features.MarketData.SyncStocks;

namespace BorsaAgent.API.Common.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapSyncStocksEndpoint();
        return app;
    }
}