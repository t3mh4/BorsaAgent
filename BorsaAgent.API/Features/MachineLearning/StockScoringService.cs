using BorsaAgent.API.Models;

namespace BorsaAgent.API.Features.MachineLearning;

public class StockScoringService
{
    // Ağırlıklar
    private const float WeightML = 0.35f;
    private const float WeightClassification = 0.15f;
    private const float WeightMomentum = 0.30f;
    private const float WeightVolume = 0.20f;

    public StockScore Calculate(StockFeature feature, float predictedReturn, float buyProbability, string code, string name)
    {
        var mlScore = CalculateMLScore(predictedReturn);
        var classScore = Math.Clamp(buyProbability, 0f, 1f); // 0–1 arası zaten
        var momentumScore = CalculateMomentumScore(feature);
        var volumeScore = CalculateVolumeScore(feature);

        var finalScore = (mlScore * WeightML) +
                    (classScore * WeightClassification) +
                    (momentumScore * WeightMomentum) +
                    (volumeScore * WeightVolume);

        return new StockScore
        {
            Code = code,
            Name = name,
            LastClose = feature.ClosePrice,
            PredictedReturn = MathF.Round(predictedReturn, 2),
            EstimatedPrice = MathF.Round(feature.ClosePrice * (1 + predictedReturn / 100), 2),
            MLScore = MathF.Round(mlScore, 4),
            BuyProbability = MathF.Round(buyProbability, 4),
            MomentumScore = MathF.Round(momentumScore, 4),
            VolumeScore = MathF.Round(volumeScore, 4),
            FinalScore = MathF.Round(finalScore, 4),
            Signal = GetSignal(finalScore)
        };
    }

    // ─── ML Score (0 – 1 arası normalize) ────
    // PredictedReturn: 0% → 0.0, 10% → 1.0
    private static float CalculateMLScore(float predictedReturn)
        => Math.Clamp(predictedReturn / 10f, 0f, 1f);

    // ─── Momentum Score ────
    private static float CalculateMomentumScore(StockFeature f)
    {
        float score = 0f;

        // SMA5 > SMA20 → güçlü trend (0.5 puan)
        if (f.SMA5 > f.SMA20)
            score += 0.5f;

        // Fiyat SMA5 üstünde → kısa vade pozitif (0.3 puan)
        if (f.ClosePrice > f.SMA5)
            score += 0.3f;

        // Günlük getiri pozitif → momentum devam ediyor (0.2 puan)
        if (f.DailyReturn > 0)
            score += 0.2f;

        return score; // max 1.0
    }

    // ─── Volume Score ────
    private static float CalculateVolumeScore(StockFeature f)
    {
        return f.Volume switch
        {
            > 10_000_000 => 1.0f,   // Çok yüksek hacim
            > 5_000_000 => 0.75f,  // Yüksek hacim
            > 2_000_000 => 0.50f,  // Orta hacim
            > 1_000_000 => 0.25f,  // Minimum eşik
            _ => 0f      // Yetersiz hacim
        };
    }

    // ─── Signal ────
    private static string GetSignal(float finalScore) => finalScore switch
    {
        >= 0.75f => "🟢 Güçlü Al",
        >= 0.55f => "🟡 Al",
        >= 0.35f => "🟠 Nötr",
        _ => "🔴 Zayıf"
    };
}