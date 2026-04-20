using System.Text.Json;

namespace BorsaAgent.API.Infrastructure.TwelveData;

public class TwelveDataClient : ITwelveDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwelveDataClient> _logger;

    public TwelveDataClient(HttpClient httpClient, ILogger<TwelveDataClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<TwelveDataStockInfo>> GetBistStocksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("TwelveData'dan BIST hisseleri çekiliyor...");

            var endpoint = $"?exchange=BIST";
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("TwelveData API hatası: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<TwelveDataResponse>(content, options);

            _logger.LogInformation("{Count} hisse başarıyla çekildi", data?.Data.Count ?? 0);
            return data?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TwelveData'dan hisse çekme sırasında hata oluştu");
            return [];
        }
    }
}