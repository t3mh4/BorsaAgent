using Microsoft.ML.Data;

namespace BorsaAgent.API.Models;

public class StockFeatureClassification
{
    [ColumnName("Label")]
    public bool Label { get; set; }

    public float DailyReturn { get; set; }
    public float PriceToSMA5 { get; set; }
    public float PriceToSMA20 { get; set; }
    public float SMA5ToSMA20 { get; set; }
    public float VolumeChange { get; set; }
    public float ClosePrice_Lag1 { get; set; }
    public float ClosePrice_Lag2 { get; set; }
    public float ClosePrice_Lag3 { get; set; }
    public float Volume_Lag1 { get; set; }
    public float HighLowRange { get; set; }
    public float OpenToClose { get; set; }
    public float RSI_14 { get; set; }
    public float Momentum_5 { get; set; }
    public float VolatilityRatio { get; set; }
}

public class StockClassificationPrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    [ColumnName("Probability")]
    public float Probability { get; set; }

    [ColumnName("Score")]
    public float Score { get; set; }
}