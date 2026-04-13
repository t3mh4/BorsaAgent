using BorsaAgent.API.Data;
using BorsaAgent.API.Features.DataCollector;
using BorsaAgent.API.Features.MachineLearning;
using BorsaAgent.API.Features.StockImporter;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
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

    public static IServiceCollection AddDataCollectorServices(this IServiceCollection services)
    {
        services.AddScoped<StockImporterService>();
        services.AddScoped<YahooFinanceService>();
        return services;
    }

    public static IServiceCollection AddMlTrainingService(this IServiceCollection services)
    {
        services.AddScoped<MLTrainingService>();
        return services;
    }

    public static IServiceCollection AddMlPredictionService(this IServiceCollection services)
    {
        services.AddScoped<MlPredictionService>();
        return services;
    }

    public static IServiceCollection AddMlBacktestService(this IServiceCollection services)
    {
        services.AddScoped<MLBacktestService>();
        return services;
    }
}