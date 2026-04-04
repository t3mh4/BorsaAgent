namespace BorsaAgent.API.Models;
public class StockFeature
{
    public int StockId { get; set; }
    public DateTime TradeDate { get; set; }

    public float ClosePrice { get; set; }
    public float OpenPrice { get; set; }
    public float HighPrice { get; set; }
    public float LowPrice { get; set; }
    public float Volume { get; set; }

    public float ClosePrice_Lag1 { get; set; }
    public float ClosePrice_Lag2 { get; set; }
    public float ClosePrice_Lag3 { get; set; }

    public float SMA5 { get; set; }
    public float SMA20 { get; set; }

    public float Volume_Lag1 { get; set; }

    public float DailyReturn { get; set; }

    public float NextDayReturn { get; set; }
}