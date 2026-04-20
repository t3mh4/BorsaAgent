using BorsaAgent.API.Infrastructure.TwelveDataClient;

namespace BorsaAgent.API.Common.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ITwelveDataApiClient, TwelveDataApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://api.twelvedata.com/stocks");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "BorsaAgentV3/1.0");
            });

        //services.AddHttpClient<IIsYatirimApiClient, IsYatirimApiClient>()
        //    .ConfigureHttpClient(client =>
        //    {
        //        client.Timeout = TimeSpan.FromSeconds(30);
        //        client.DefaultRequestHeaders.Add("User-Agent", "BorsaAgentV3/1.0");
        //    });
        return services;
    }
}