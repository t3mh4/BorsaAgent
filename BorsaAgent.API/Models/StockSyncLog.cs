namespace BorsaAgent.API.Models;

public class StockSyncLog
{
    public long Id { get; set; }
    public int StockId { get; set; }
    public Stock Stock { get; set; }

    public string RequestUrl { get; set; } = string.Empty;  // full Yahoo URL
    public string ErrorMessage { get; set; } = string.Empty;

    public DateTimeOffset Period1Utc { get; set; }
    public DateTimeOffset Period2Utc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}