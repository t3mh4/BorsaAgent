using System.Text.Json;

namespace BorsaAgent.API.Infrastructure.TwelveDataClient;

public class TwelveDataApiClient : ITwelveDataApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwelveDataApiClient> _logger;

    public TwelveDataApiClient(HttpClient httpClient, ILogger<TwelveDataApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<StockInfo>> GetBistStocksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("TwelveData'dan BIST hisseleri çekiliyor...");

            var url = $"https://api.twelvedata.com/stocks?exchange=BIST";
            var response = await _httpClient.GetAsync(url, cancellationToken);

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