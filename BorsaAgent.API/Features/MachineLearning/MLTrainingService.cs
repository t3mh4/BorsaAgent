using BorsaAgent.API.Constanst;
using BorsaAgent.API.Data;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace BorsaAgent.API.Features.MachineLearning;

public class MLTrainingService(IDbContextFactory<AppDbContext> dbFactory, ILogger<MLTrainingService> logger)
{
    private readonly MLContext _mlContext = new(seed: 42);
    private const string RegressionModelPath = "stock_model_regression.zip";
    private const string ClassificationModelPath = "stock_model_classification.zip";

    public async Task TrainModelAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Model eğitimi başlıyor...");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var newStockIds = await db.DailyPrices.GroupBy(x => x.StockId)
            .Where(g => g.Count() < StockFilterConstants.MinListingDays)     
            .Select(g => g.Key)
            .ToListAsync(ct);

        var data = await db.StockFeatures
            .AsNoTracking()
            .Where(x => !newStockIds.Contains(x.StockId)
                        && x.NextDayReturn != 0
                        && x.NextDayReturn > -15
                        && x.NextDayReturn < 15
                        && x.PriceToSMA5 > 0.5
                        && x.PriceToSMA5 < 1.5
                        && x.PriceToSMA20 > 0.5
                        && x.PriceToSMA20 < 1.5
                        && x.VolumeChange > 0
                        && x.VolumeChange < 20
                        && x.DailyReturn > -15
                        && x.DailyReturn < 15
                        && x.HighLowRange > 0
                        && x.HighLowRange < 0.15
                        && x.OpenToClose > -10
                        && x.OpenToClose < 10
                        && x.ClosePrice_Lag1 > 0
                        && x.ClosePrice_Lag2 > 0
                        && x.ClosePrice_Lag3 > 0
                        && x.Volume_Lag1 > 0)
            .ToListAsync(ct);

        if (data.Count == 0)
        {
            logger.LogWarning("Eğitim verisi yok!");
            return;
        }

        logger.LogInformation("Toplam veri: {Count}", data.Count);

        // ── 1. REGRESYON MODELİ ────
        await TrainRegressionAsync(data, db, ct);

        // ── 2. CLASSİFİCATİON MODELİ ────
        await TrainClassificationAsync(data, db, ct);
    }

    // ────
    private async Task TrainRegressionAsync(List<StockFeature> data,
        AppDbContext db, CancellationToken ct)
    {
        logger.LogInformation("Regresyon modeli eğitiliyor...");

        var dataView = _mlContext.Data.LoadFromEnumerable(data);
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        var pipeline =
            _mlContext.Transforms.CopyColumns("Label", nameof(StockFeature.NextDayReturn))
            .Append(_mlContext.Transforms.Concatenate("Features",
                nameof(StockFeature.DailyReturn),
                nameof(StockFeature.PriceToSMA5),
                nameof(StockFeature.PriceToSMA20),
                nameof(StockFeature.SMA5ToSMA20),
                nameof(StockFeature.VolumeChange),
                nameof(StockFeature.ClosePrice_Lag1),
                nameof(StockFeature.ClosePrice_Lag2),
                nameof(StockFeature.ClosePrice_Lag3),
                nameof(StockFeature.Volume_Lag1),
                nameof(StockFeature.HighLowRange),
                nameof(StockFeature.OpenToClose),
                nameof(StockFeature.RSI_14),
                nameof(StockFeature.Momentum_5),
                nameof(StockFeature.VolatilityRatio),
                nameof(StockFeature.Bollinger_PercentB),
                nameof(StockFeature.MACD_Hist),
                nameof(StockFeature.ATR_Percent)
            ))
            .Append(_mlContext.Transforms.NormalizeMeanVariance("Features"))
            .Append(_mlContext.Regression.Trainers.LightGbm(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 31,
                numberOfIterations: 150,
                minimumExampleCountPerLeaf: 40,
                learningRate: 0.08
            ));

        var model = pipeline.Fit(split.TrainSet);
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.Regression.Evaluate(predictions);

        logger.LogInformation("Regresyon → R²: {R2}, RMSE: {Rmse}", metrics.RSquared, metrics.RootMeanSquaredError);

        var metric = new MlModelMetric
        {
            RSquared = Math.Round(metrics.RSquared, 6),
            Rmse = Math.Round(metrics.RootMeanSquaredError, 6),
            Mae = Math.Round(metrics.MeanAbsoluteError, 6),
            TrainingDataCount = data.Count,
            ModelPath = RegressionModelPath,
            Notes = "regression",
            CreatedAtUtc = DateTime.UtcNow
        };
        await db.MlModelMetrics.AddAsync(metric, ct);
        await db.SaveChangesAsync(ct);

        _mlContext.Model.Save(model, dataView.Schema, RegressionModelPath);
        logger.LogInformation("Regresyon modeli kaydedildi: {Path}", RegressionModelPath);
    }

    // ────
    private async Task TrainClassificationAsync(List<StockFeature> data,
        AppDbContext db, CancellationToken ct)
    {
        logger.LogInformation("Classification modeli eğitiliyor...");

        // NextDayReturn > 1.5 → true (Al), değilse false
        var classData = data.Select(x => new StockFeatureClassification
        {
            Label = x.NextDayReturn > 1.5f,
            DailyReturn = x.DailyReturn,
            PriceToSMA5 = x.PriceToSMA5,
            PriceToSMA20 = x.PriceToSMA20,
            SMA5ToSMA20 = x.SMA5ToSMA20,
            VolumeChange = x.VolumeChange,
            ClosePrice_Lag1 = x.ClosePrice_Lag1,
            ClosePrice_Lag2 = x.ClosePrice_Lag2,
            ClosePrice_Lag3 = x.ClosePrice_Lag3,
            Volume_Lag1 = x.Volume_Lag1,
            HighLowRange = x.HighLowRange,
            OpenToClose = x.OpenToClose,
            RSI_14 = x.RSI_14,
            Momentum_5 = x.Momentum_5,
            ATR_Percent = x.ATR_Percent,
            Bollinger_PercentB = x.Bollinger_PercentB,
            MACD_Hist = x.MACD_Hist
        }).ToList();

        var dataView = _mlContext.Data.LoadFromEnumerable(classData);
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        var pipeline =
            _mlContext.Transforms.Concatenate("Features",
                nameof(StockFeatureClassification.DailyReturn),
                nameof(StockFeatureClassification.PriceToSMA5),
                nameof(StockFeatureClassification.PriceToSMA20),
                nameof(StockFeatureClassification.SMA5ToSMA20),
                nameof(StockFeatureClassification.VolumeChange),
                nameof(StockFeatureClassification.ClosePrice_Lag1),
                nameof(StockFeatureClassification.ClosePrice_Lag2),
                nameof(StockFeatureClassification.ClosePrice_Lag3),
                nameof(StockFeatureClassification.Volume_Lag1),
                nameof(StockFeatureClassification.HighLowRange),
                nameof(StockFeatureClassification.OpenToClose),
                nameof(StockFeatureClassification.RSI_14),
                nameof(StockFeatureClassification.Momentum_5),
                nameof(StockFeatureClassification.VolatilityRatio)
            )
            .Append(_mlContext.Transforms.NormalizeMeanVariance("Features"))
            .Append(_mlContext.BinaryClassification.Trainers.LightGbm(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 31,
                numberOfIterations: 150,
                minimumExampleCountPerLeaf: 40,
                learningRate: 0.08
            ));

        var model = pipeline.Fit(split.TrainSet);
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.BinaryClassification.Evaluate(predictions);

        logger.LogInformation("Classification → Accuracy: {Acc}, AUC: {Auc}",
            metrics.Accuracy, metrics.AreaUnderRocCurve);

        var metric = new MlModelMetric
        {
            RSquared = Math.Round(metrics.AreaUnderRocCurve, 6), // AUC burada
            Rmse = 0,
            Mae = 0,
            TrainingDataCount = data.Count,
            ModelPath = ClassificationModelPath,
            Notes = $"classification | accuracy={Math.Round(metrics.Accuracy, 4)} | auc={Math.Round(metrics.AreaUnderRocCurve, 4)}",
            CreatedAtUtc = DateTime.UtcNow
        };
        await db.MlModelMetrics.AddAsync(metric, ct);
        await db.SaveChangesAsync(ct);

        _mlContext.Model.Save(model, dataView.Schema, ClassificationModelPath);
        logger.LogInformation("Classification modeli kaydedildi: {Path}", ClassificationModelPath);
    }
}