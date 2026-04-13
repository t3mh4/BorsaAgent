namespace BorsaAgent.API.Features.MachineLearning;

public static class MachineLearningEndpoints
{
    public static void MapMachineLearningEndpoints(this WebApplication app)
    {
        app.MapPost("/ml/train", async (MLTrainingService service,
                    CancellationToken ct) =>
        {
            await service.TrainModelAsync(ct);
            return Results.Ok("Eğitim tamamlandı.");
        });

        // Tek hisse tahmini
        app.MapGet("/ml/predict/{code}", async (string code, MlPredictionService service) =>
        {
            var result = await service.PredictAsync(code);
            return result is null ? Results.NotFound($"{code} bulunamadı veya filtre kriterlerini karşılamıyor.") : Results.Ok(result);
        });

        // Top 10 listesi
        app.MapGet("/ml/top-picks", async (MlPredictionService service) =>
        {
            var picks = await service.GetTopPicksAsync(count: 10);
            var response = new
            {
                descriptions = new
                {
                    code = "Hissenin Borsa İstanbul'daki ticker kodu (ör: ATATR.IS)",
                    name = "Şirketin tam adı",
                    lastClose = "Modelin tahmin için kullandığı son kapanış fiyatı (TL)",
                    predictedReturn = "Regression modelinin tahmin ettiği ertesi gün getirisi (%)",
                    estimatedPrice = "Tahmin edilen ertesi gün kapanış fiyatı: lastClose × (1 + predictedReturn / 100)",
                    mlScore = "predictedReturn'den normalize edilmiş ML güven skoru (0-1 arası)",
                    momentumScore = "Son günlerdeki fiyat momentumuna göre hesaplanan skor (0-1 arası)",
                    volumeScore = "Hacmin ortalamayla kıyaslanmasına göre hesaplanan skor (0-1 arası)",
                    buyProbability = "Classification modelinin 'bu hisse yarın >%1.5 yükselir' deme olasılığı (0-1 arası). 0.5 altı = belirsiz, 0.7 üstü = güçlü sinyal",
                    finalScore = "mlScore, buyProbability, momentumScore ve volumeScore'un ağırlıklı ortalaması (0-1 arası). Yükseldikçe sinyal güçlenir",
                    signal = "finalScore'a göre üretilen sinyal: 🟢 Güçlü Al / 🟡 Al / 🟠 Nötr / 🔴 Sat"
                },
                picks
            };
            return Results.Ok(response);
        });

        app.MapGet("/ml/backtest", async (int? days, MLBacktestService service) =>
        {
            var report = await service.RunBacktestAsync(days ?? 15);
            return Results.Ok(report);
        });
    }
}