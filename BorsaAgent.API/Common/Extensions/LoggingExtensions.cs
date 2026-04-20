using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace BorsaAgent.API.Common.Extensions;

public static class LoggingExtensions
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Hangi kolonlar oluşturulsun?
        var columnOptions = new Dictionary<string, ColumnWriterBase>
        {
            { "Id",          new IdAutoIncrementColumnWriter() },
            { "Message",     new RenderedMessageColumnWriter() },
            { "Level",       new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "Timestamp",   new TimestampColumnWriter() },
            { "Exception",   new ExceptionColumnWriter() },
            { "Properties",  new PropertiesColumnWriter() },  // structured log alanları (StockCode vs.)
            { "MachineName", new SinglePropertyColumnWriter("MachineName") }
        };

        builder.Host.UseSerilog((ctx, services, config) =>
            config.ReadFrom.Configuration(ctx.Configuration)
                  .ReadFrom.Services(services)
                  .WriteTo.PostgreSQL(
                      connectionString: connectionString,
                      tableName: "logs",
                      columnOptions: columnOptions,
                      needAutoCreateTable: true,
                      restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
                  ));
    }
}