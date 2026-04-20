namespace BorsaAgent.API.Infrastructure.TwelveDataClient;
public interface ITwelveDataApiClient
{
    /// <summary>
    /// BIST borsasındaki tüm hisseleri çeker
    /// </summary>
    Task<List<StockInfo>> GetBistStocksAsync(CancellationToken cancellationToken = default);
}
