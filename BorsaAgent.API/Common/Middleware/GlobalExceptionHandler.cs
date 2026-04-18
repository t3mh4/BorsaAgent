using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace BorsaAgent.API.Common.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                Exception exception,
                                                CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            OperationCanceledException => (HttpStatusCode.BadRequest, "İşlem iptal edildi."),
            ArgumentException e => (HttpStatusCode.BadRequest, e.Message),
            KeyNotFoundException e => (HttpStatusCode.NotFound, e.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim."),
            _ => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.")
        };

        // Loglama seviyesi: cancel → Warning, diğerleri → Error
        if (exception is OperationCanceledException)
            logger.LogWarning("İşlem iptal edildi. Path: {Path}", httpContext.Request.Path);
        else
            logger.LogError(exception,
                "Hata oluştu. Path: {Path} | StatusCode: {StatusCode} | Message: {Message}",
                httpContext.Request.Path,
                (int)statusCode,
                exception.Message);

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            // Development'ta detay göster, Production'da gizle
            Detail = httpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment()
                ? exception.ToString()
                : null
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response),
            cancellationToken);

        return true;
    }
}