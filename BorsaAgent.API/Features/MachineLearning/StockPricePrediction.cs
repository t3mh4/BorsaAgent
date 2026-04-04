using Microsoft.ML.Data;

namespace BorsaAgent.API.Features.MachineLearning;

public class StockPricePrediction
{
    // ML.NET Regression'da tahmin edilen kolonun varsayılan adı "Score"dur.
    [ColumnName("Score")]
    public float PredictedValue { get; set; }
}