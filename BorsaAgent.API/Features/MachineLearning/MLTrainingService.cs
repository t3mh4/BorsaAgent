using BorsaAgent.API.Data;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace BorsaAgent.API.Features.MachineLearning;

public class MLTrainingService(
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<MLTrainingService> logger)
{
    private readonly MLContext _mlContext = new(seed: 42);
    private const string ModelPath = "stock_model.zip";

    public async Task TrainModelAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Model eğitimi başlıyor...");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var data = await db.StockFeatures
            .AsNoTracking()
            .Where(x => x.NextDayReturn > 0) // güvenlik filtresi
            .ToListAsync(ct);

        if (data.Count == 0)
        {
            logger.LogWarning("Eğitim verisi yok!");
            return;
        }

        logger.LogInformation("Toplam veri: {Count}", data.Count);

        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        // ✅ Train / Test split
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        var pipeline =
            _mlContext.Transforms.CopyColumns("Label", nameof(StockFeature.NextDayReturn))
            .Append(_mlContext.Transforms.Concatenate("Features",
                nameof(StockFeature.ClosePrice),
                nameof(StockFeature.OpenPrice),
                nameof(StockFeature.HighPrice),
                nameof(StockFeature.LowPrice),
                nameof(StockFeature.Volume),
                nameof(StockFeature.ClosePrice_Lag1),
                nameof(StockFeature.ClosePrice_Lag2),
                nameof(StockFeature.ClosePrice_Lag3),
                nameof(StockFeature.SMA5),
                nameof(StockFeature.SMA20),
                nameof(StockFeature.DailyReturn)
            ))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.FastTree());

        logger.LogInformation("Model eğitiliyor...");
        var model = pipeline.Fit(split.TrainSet);

        logger.LogInformation("Model test ediliyor...");
        var predictions = model.Transform(split.TestSet);

        var metrics = _mlContext.Regression.Evaluate(predictions);
        var metric = new MlModelMetric
        {
            RSquared = Math.Round(metrics.RSquared, 6),
            Rmse = Math.Round(metrics.RootMeanSquaredError, 6),
            Mae = Math.Round(metrics.MeanAbsoluteError, 6),
            TrainingDataCount = data.Count,
            ModelPath = ModelPath,
            Notes = "",
            CreatedAtUtc = DateTime.UtcNow
        };
        await db.MlModelMetrics.AddAsync(metric,ct);
        await db.SaveChangesAsync(ct);

        // ✅ Model kaydet
        _mlContext.Model.Save(model, dataView.Schema, ModelPath);

        logger.LogInformation("Model kaydedildi: {Path}", ModelPath);
    }
}