using BorsaAgent.API.Infrastructure.IsYatirim;
using BorsaAgent.API.Infrastructure.TwelveData;

namespace BorsaAgent.API.Common.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ITwelveDataClient, TwelveDataClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://api.twelvedata.com/stocks");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "BorsaAgentV3/1.0");
            });

        services.AddHttpClient<IIsYatirimClient, IsYatirimClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://www.isyatirim.com.tr/_layouts/15/Isyatirim.Website/Common/Data.aspx/HisseTekil");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "BorsaAgentV3/1.0");
            });
        return services;
    }
}