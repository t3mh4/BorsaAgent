using BorsaAgent.API.Features.DataCollector;
using BorsaAgent.API.Features.MachineLearning;

namespace BorsaAgent.API.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapCollectorEndpoints();
        app.MapMachineLearningEndpoints();
        // İleride eklenecekler buraya gelecek
        // app.MapStockEndpoints();
        // app.MapPredictionEndpoints();
        // app.MapAgentEndpoints();

        return app;
    }
}