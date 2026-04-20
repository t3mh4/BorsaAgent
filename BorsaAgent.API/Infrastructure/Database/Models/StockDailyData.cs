namespace BorsaAgent.API.Infrastructure.Database.Models;


/// <summary>
/// Günlük hisse verisi (İş Yatırım)
/// </summary>
public class StockDailyData
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Stock Stock { get; set; }
}
