namespace BorsaAgent.API.Infrastructure.Database.Models;


/// <summary>
/// Midas snapshot verisi (anlık detaylı veri)
/// </summary>
public class MidasSnapshot
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public DateTime Date { get; set; }

    // OHLC
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Last { get; set; }
    public decimal? Close { get; set; }
    public decimal? PreviousClose { get; set; }

    // Change
    public decimal? DailyChange { get; set; }
    public decimal? DailyChangePercent { get; set; }

    // Volume & Turnover
    public long? TotalVolume { get; set; }
    public decimal? TotalTurnover { get; set; }

    // Bid/Ask
    public decimal? Ask { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Vwap { get; set; }

    // Yearly
    public decimal? YearlyChange { get; set; }

    // Limits
    public decimal? LowerLimit { get; set; }
    public decimal? UpperLimit { get; set; }

    // Weekly
    public decimal? WeeklyChange { get; set; }
    public decimal? WeeklyChangePercent { get; set; }

    // Monthly
    public decimal? MonthlyChange { get; set; }
    public decimal? MonthlyChangePercent { get; set; }
    public decimal? YearlyChangePercent { get; set; }

    // Risk & Fundamental
    public decimal? Volatility { get; set; }
    public decimal? PriceEarning { get; set; }
    public decimal? PriceBookValue { get; set; }
    public decimal? ReturnOnEquity { get; set; }
    public long? MarketValue { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Stock Stock { get; set; }
}
