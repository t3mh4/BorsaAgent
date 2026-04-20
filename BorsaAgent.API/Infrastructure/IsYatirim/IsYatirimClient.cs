using System.Text.Json;
using BorsaAgent.API.Infrastructure.IsYatirim;

namespace BorsaAgent.API.Infrastructure.IsYatirim;

public class IsYatirimClient : IIsYatirimClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IsYatirimClient> _logger;

    public IsYatirimClient(HttpClient httpClient, ILogger<IsYatirimClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<IsYatirimDailyData>> GetHistoricalDataAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("İş Yatırım'dan {Symbol} için veri çekiliyor. Tarih: {StartDate} - {EndDate}",
                symbol, startDate.Date, endDate.Date);

            var endpoint = $"?hisse={symbol}&startdate={startDate:dd-MM-yyyy}&enddate={endDate:dd-MM-yyyy}";

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("İş Yatırım API hatası: {StatusCode} - {Symbol}", response.StatusCode, symbol);
                return new List<IsYatirimDailyData>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<IsYatirimResponse>(content, options);

            if (apiResponse?.Ok == false)
            {
                _logger.LogWarning("İş Yatırım API hatası: {ErrorCode} - {ErrorDescription}",
                    apiResponse.ErrorCode, apiResponse.ErrorDescription);
                return new List<IsYatirimDailyData>();
            }

            _logger.LogInformation("{Symbol} için {Count} gün veri çekildi", symbol, apiResponse?.Value.Count ?? 0);
            return apiResponse?.Value ?? new List<IsYatirimDailyData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İş Yatırım'dan veri çekme sırasında hata oluştu. Symbol: {Symbol}", symbol);
            return new List<IsYatirimDailyData>();
        }
    }
}