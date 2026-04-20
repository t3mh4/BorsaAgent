namespace BorsaAgent.API.Infrastructure.TwelveData;
public interface ITwelveDataClient
{
    /// <summary>
    /// BIST borsasındaki tüm hisseleri çeker
    /// </summary>
    Task<List<TwelveDataStockInfo>> GetBistStocksAsync(CancellationToken cancellationToken = default);
}
