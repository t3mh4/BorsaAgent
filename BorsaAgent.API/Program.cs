using BorsaAgent.API.Common.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .CreateBootstrapLogger();


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging();                    // ← Serilog
    builder.Services.AddGlobalExceptionHandling();  // ← Global exception handler

    // --- 1. Servis Kayıtları (Dependency Injection) ---

    // Veritabanı (PostgreSQL)
    builder.Services.AddDatabase(builder.Environment);

    // HttpClientFactory
    builder.Services.AddHttpClients();

    // Feature Servisleri

    // CORS Politikası (n8n veya Frontend erişimi için)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Swagger servislerini her zaman kaydetmek (Build hatası almamak için) güvenlidir, 
    // asıl kısıtlamayı aşağıda Middleware kısmında yapacağız.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0}ms)";
    });
    // --- 2. Middleware Pipeline ---

    // SADECE Development ortamında Swagger'ı aktif et ✅
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Borsa Agent V1 API v1");
            c.RoutePrefix = "swagger"; // Tarayıcıda: https://localhost:xxxx/swagger
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // --- 3. Endpoint Mapping ---
    app.MapAllEndpoints();

    app.MapGet("/", () => new { Status = "Online", Project = "Borsa Agent Init", Version = "0.0.0" });

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılamadı.");
}
finally
{
    await Log.CloseAndFlushAsync();
}