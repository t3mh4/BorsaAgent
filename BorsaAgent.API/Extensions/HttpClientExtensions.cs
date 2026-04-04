namespace BorsaAgent.API.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("TwelveData", client =>
        {
            client.BaseAddress = new Uri("https://api.twelvedata.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Yahoo", client =>
        {
            client.BaseAddress = new Uri("https://query1.finance.yahoo.com/v8/finance/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        });

        return services;
    }
}