using BorsaAgent.API.Features.MarketData.SyncHistoricalData;
using BorsaAgent.API.Features.MarketData.SyncStocks;
using BorsaAgent.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Common.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddDbContextFactory<BorsaAgentDbContext>(options =>
        {
            options.UseNpgsql(DatabaseConfiguration.GetConnectionString());

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });
        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<SyncStocksHandler>();
        services.AddScoped<SyncHistoricalDataHandler>();
        return services;
    }
}