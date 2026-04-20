using BorsaAgent.API.Infrastructure.IsYatirim;

namespace BorsaAgent.API.Infrastructure.IsYatirim;

public interface IIsYatirimClient
{
    Task<List<IsYatirimDailyData>> GetHistoricalDataAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}