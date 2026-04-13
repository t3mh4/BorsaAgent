using BorsaAgent.API.Constanst;
using BorsaAgent.API.Data;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace BorsaAgent.API.Features.MachineLearning;

public class MlPredictionService(IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly StockScoringService _scoringService = new();
    private readonly MLContext _mlContext = new(seed: 42);

    private PredictionEngine<StockFeature, StockPricePrediction>? _regressionEngine;
    private PredictionEngine<StockFeatureClassification, StockClassificationPrediction>? _classificationEngine;

    private const string RegressionModelPath = "stock_model_regression.zip";
    private const string ClassificationModelPath = "stock_model_classification.zip";

    private static StockFeatureClassification ToClassificationInput(StockFeature x) => new()
    {
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
        VolatilityRatio = x.VolatilityRatio,
        Bollinger_PercentB = x.Bollinger_PercentB,
        MACD_Hist = x.MACD_Hist,
        ATR_Percent = x.ATR_Percent
    };

    // ── Predict (tek hisse) ────
    public async Task<StockScore> PredictAsync(string stockCode, CancellationToken ct = default)
    {
        LoadModels();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var stock = await db.Stocks.FirstOrDefaultAsync(s => s.ShortCode == stockCode, ct);
        if (stock == null) return null;

        var maxDate = await db.DailyPrices.MaxAsync(x => (DateOnly?)x.TradeDate, ct);
        if (!maxDate.HasValue) return new();
        var lastPriceDateTime = DateTime.SpecifyKind(maxDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var raw = await db.StockFeatures
            .AsNoTracking()
            .Where(x => x.StockId == stock.Id
                    && x.TradeDate == lastPriceDateTime
                    && x.ClosePrice > StockFilterConstants.MinClosePrice
                    && x.Volume > StockFilterConstants.MinVolume)
            .FirstOrDefaultAsync(ct);

        if (raw == null) return null;

        var regPrediction = _regressionEngine!.Predict(raw);
        var clsPrediction = _classificationEngine!.Predict(ToClassificationInput(raw));

        return _scoringService.Calculate(raw, regPrediction.PredictedValue, clsPrediction.Probability, stock.Code, stock.Name);
    }

    // ── GetTopPicks ────
    public async Task<List<StockScore>> GetTopPicksAsync(int count = 10, DateOnly? targetDate = null, CancellationToken ct = default)
    {
        LoadModels();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        DateOnly finalDate = targetDate ?? (await db.DailyPrices.MaxAsync(x => (DateOnly?)x.TradeDate, ct) ?? DateOnly.FromDateTime(DateTime.Now));
        var lastPriceDateTime = DateTime.SpecifyKind(finalDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var rawFeatures = await db.StockFeatures
            .AsNoTracking()
            .Where(x => x.TradeDate == lastPriceDateTime
                    && x.ClosePrice > StockFilterConstants.MinClosePrice
                    && x.Volume > StockFilterConstants.MinVolume
                    && x.ClosePrice > x.SMA5
                    && x.ClosePrice > x.SMA20
                    && x.SMA5 > x.SMA20)
            .ToListAsync(ct);

        var scores = new List<StockScore>();

        foreach (var raw in rawFeatures)
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Id == raw.StockId, ct);
            if (stock == null) continue;

            var regPrediction = _regressionEngine!.Predict(raw);
            var clsPrediction = _classificationEngine!.Predict(ToClassificationInput(raw));
            if (clsPrediction.Probability < 0.52f)
                continue;
            scores.Add(_scoringService.Calculate(raw, regPrediction.PredictedValue, clsPrediction.Probability, stock.Code, stock.Name));
        }

        return scores
            .OrderByDescending(x => x.FinalScore)
            .Take(count)
            .ToList();
    }

    // ── Model Yükleme ────
    public void LoadModels()
    {
        if (_regressionEngine == null && File.Exists(RegressionModelPath))
        {
            var model = _mlContext.Model.Load(RegressionModelPath, out _);
            _regressionEngine = _mlContext.Model.CreatePredictionEngine<StockFeature, StockPricePrediction>(model);
        }

        if (_classificationEngine == null && File.Exists(ClassificationModelPath))
        {
            var model = _mlContext.Model.Load(ClassificationModelPath, out _);
            _classificationEngine = _mlContext.Model.CreatePredictionEngine<StockFeatureClassification, StockClassificationPrediction>(model);
        }
    }
}