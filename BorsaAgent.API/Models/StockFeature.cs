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

    // ✅ Yeni Ratio Feature'lar
    public float PriceToSMA5 { get; set; }
    public float PriceToSMA20 { get; set; }
    public float SMA5ToSMA20 { get; set; }
    public float VolumeChange { get; set; }
    // ✅ Gün içi Feature'lar
    public float HighLowRange { get; set; }  // (High - Low) / Close — volatilite
    public float OpenToClose { get; set; }   // (Close - Open) / Open * 100 — gün içi yön

    // ✅ Yeni Teknik İndikatörler
    public float RSI_14 { get; set; }
    public float Momentum_5 { get; set; }
    public float VolatilityRatio { get; set; }

    public float Bollinger_PercentB { get; set; }
    public float MACD_Hist { get; set; }
    public float ATR_Percent { get; set; }


    // ✅ Classification Label (NextDayReturn > 1.5 → 1, değilse 0)
    [Microsoft.ML.Data.NoColumn]
    public bool IsPositive => NextDayReturn > 1.5f;
}