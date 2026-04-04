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
            (await service.GetPredictionsAsync(stockCode: code)).FirstOrDefault());

        // Top 10 listesi
        app.MapGet("/ml/top-picks", async (MlPredictionService service) =>
            await service.GetPredictionsAsync(topCount: 10));
    }
}
