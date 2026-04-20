using BorsaAgent.API.Features.MarketData.SyncStocks;
using BorsaAgent.API.Features.MarketData.SyncHistoricalData;

namespace BorsaAgent.API.Common.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapSyncStocksEndpoint();
        app.MapSyncHistoricalDataEndpoint();
        return app;
    }
}