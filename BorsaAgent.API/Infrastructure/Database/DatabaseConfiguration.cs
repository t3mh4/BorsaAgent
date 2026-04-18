namespace BorsaAgent.API.Infrastructure.Database;

public static class DatabaseConfiguration
{
    public static string GetConnectionString()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();

        IConfigurationRoot configuration = builder.Build();

        return configuration.GetConnectionString("DefaultConnection");
    }
}
