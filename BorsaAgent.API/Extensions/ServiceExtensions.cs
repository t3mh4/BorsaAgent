using BorsaAgent.API.Data;
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
}