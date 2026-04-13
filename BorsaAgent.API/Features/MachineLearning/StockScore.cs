namespace BorsaAgent.API.Features.MachineLearning;

public record StockScore
{
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public float LastClose { get; init; }
    public float PredictedReturn { get; init; }
    public float EstimatedPrice { get; init; }

    // Alt skorlar (debug/şeffaflık için)
    public float MLScore { get; init; }
    public float MomentumScore { get; init; }
    public float VolumeScore { get; init; }
    public float FinalScore { get; init; }

    // İnsan dostu açıklama
    public string Signal { get; init; } = "";

    // ML classification çıktısı
    public float BuyProbability { get; init; }
}