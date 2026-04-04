using BorsaAgent.API.Data;
using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace BorsaAgent.API.Features.MachineLearning;

public class MlPredictionService(IDbContextFactory<AppDbContext> dbFactory)
{
    // Ortak bir "Tahmin Sonucu" modeli (Hücre bazlı)
    public record PredictionResult(string Code, string Name, float LastClose, float PredictedReturn, float EstimatedPrice);

    public async Task<List<PredictionResult>> GetPredictionsAsync(string stockCode = null, int? topCount = null, CancellationToken ct = default)
    {
        if (_predictionEngine == null) LoadModel();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // 1. En son işlem tarihini bulalım (Tüm borsa için aynı gün)
        var maxDate = await db.DailyPrices.MaxAsync(x => (DateOnly?)x.TradeDate, ct);
        if (!maxDate.HasValue) return new();
        var lastPriceDateTime = DateTime.SpecifyKind(maxDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        // 2. Veriyi çekelim (Tek hisse veya Hepsi)
        var query = db.StockFeatures.AsNoTracking().Where(x => x.TradeDate == lastPriceDateTime);

        if (!string.IsNullOrEmpty(stockCode))
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Code == stockCode, ct);
            if (stock == null) return new();
            query = query.Where(x => x.StockId == stock.Id);
        }
        query = query.Select(x => new StockFeature
        {
            StockId = x.StockId,
            TradeDate = x.TradeDate,
            ClosePrice = x.ClosePrice,
            OpenPrice = x.OpenPrice,
            HighPrice = x.HighPrice,
            LowPrice = x.LowPrice,
            Volume = x.Volume,
            ClosePrice_Lag1 = x.ClosePrice_Lag1,
            ClosePrice_Lag2 = x.ClosePrice_Lag2,
            ClosePrice_Lag3 = x.ClosePrice_Lag3,
            SMA5 = x.SMA5,
            SMA20 = x.SMA20,
            Volume_Lag1 = x.Volume_Lag1,
            DailyReturn = x.DailyReturn
        });

        var features = await query.ToListAsync(ct);
        var results = new List<PredictionResult>();

        // 3. Tahminleri yapalım
        foreach (var feature in features)
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Id == feature.StockId, ct);
            if (stock == null) continue;

            var prediction = _predictionEngine!.Predict(feature);

            var estPrice = feature.ClosePrice * (1 + prediction.PredictedValue / 100);

            results.Add(new PredictionResult(
                stock.Code,
                stock.Name,
                feature.ClosePrice,
                prediction.PredictedValue,
                (float)Math.Round(estPrice, 2)
            ));
        }

        // 4. Eğer top-picks istenmişse sıralayalım
        if (topCount.HasValue)
        {
            return results.OrderByDescending(x => x.PredictedReturn).Take(topCount.Value).ToList();
        }

        return results;
    }

    private readonly MLContext _mlContext = new(seed: 42);
    private PredictionEngine<StockFeature, StockPricePrediction> _predictionEngine;
    private const string ModelPath = "stock_model.zip";

    public void LoadModel()
    {
        if (!File.Exists(ModelPath)) return;
        var _model = _mlContext.Model.Load(ModelPath, out _);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<StockFeature, StockPricePrediction>(_model);
    }

    //public async Task<object> PredictNextCloseAsync(string stockCode, CancellationToken ct = default)
    //{
    //    if (_predictionEngine == null) LoadModel();

    //    await using var db = await dbFactory.CreateDbContextAsync(ct);

    //    // En güncel veriyi çek (Tahmin için bugünkü veriyi girdi olarak kullanacağız)

    //    var latestFeature = await db.StockFeatures
    //        .AsNoTracking()
    //        .Where(x => x.StockId == db.Stocks.FirstOrDefault(s => s.ShortCode == stockCode).Id
    //                    && x.ClosePrice > 0
    //                    && x.ClosePrice_Lag1 > 0)
    //        .OrderByDescending(x => x.TradeDate)
    //        .Select(x => new StockFeature
    //        {
    //            StockId = x.StockId,
    //            TradeDate = x.TradeDate,
    //            ClosePrice = x.ClosePrice,
    //            OpenPrice = x.OpenPrice,
    //            HighPrice = x.HighPrice,
    //            LowPrice = x.LowPrice,
    //            Volume = x.Volume,
    //            ClosePrice_Lag1 = x.ClosePrice_Lag1,
    //            ClosePrice_Lag2 = x.ClosePrice_Lag2,
    //            ClosePrice_Lag3 = x.ClosePrice_Lag3,
    //            SMA5 = x.SMA5,
    //            SMA20 = x.SMA20,
    //            Volume_Lag1 = x.Volume_Lag1,
    //            DailyReturn = x.DailyReturn,
    //        })
    //        .FirstOrDefaultAsync(ct);

    //    if (latestFeature == null) return null;

    //    var prediction = _predictionEngine!.Predict(latestFeature);
    //    // Tahmin edilen getiriyi gerçek fiyata çevir
    //    var predictedReturn = prediction.PredictedValue;
    //    var predictedPrice = latestFeature.ClosePrice * (1 + predictedReturn / 100);

    //    return new
    //    {
    //        Stock = stockCode,
    //        LastClose = latestFeature.ClosePrice,
    //        PredictedReturn = Math.Round(predictedReturn, 4),
    //        PredictedNextClose = Math.Round(predictedPrice, 2)
    //    };
    //}
    //public async Task<List<object>> GetTopPicksAsync(int count = 10, CancellationToken ct = default)
    //{
    //    if (_predictionEngine == null) LoadModel();

    //    await using var db = await dbFactory.CreateDbContextAsync(ct);
    //    var lastPriceDate = await db.DailyPrices.MaxAsync(d => d.TradeDate, ct);
    //    var lastPriceDateTime = DateTime.SpecifyKind(lastPriceDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    //    // Her hissenin EN GÜNCEL (son gün) verisini çekiyoruz
    //    var allLatestFeatures = await db.StockFeatures
    //        .AsNoTracking()
    //        .Where(x =>x.TradeDate == lastPriceDateTime)
    //         .Select(x => new StockFeature
    //         {
    //             StockId = x.StockId,
    //             TradeDate = x.TradeDate,
    //             ClosePrice = x.ClosePrice,
    //             OpenPrice = x.OpenPrice,
    //             HighPrice = x.HighPrice,
    //             LowPrice = x.LowPrice,
    //             Volume = x.Volume,
    //             ClosePrice_Lag1 = x.ClosePrice_Lag1,
    //             ClosePrice_Lag2 = x.ClosePrice_Lag2,
    //             ClosePrice_Lag3 = x.ClosePrice_Lag3,
    //             SMA5 = x.SMA5,
    //             SMA20 = x.SMA20,
    //             Volume_Lag1 = x.Volume_Lag1,
    //             DailyReturn = x.DailyReturn,
    //         })
    //        .ToListAsync(ct);

    //    var results = new List<object>();

    //    foreach (var feature in allLatestFeatures)
    //    {
    //        var prediction = _predictionEngine!.Predict(feature);
    //        var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Id == feature.StockId, ct);

    //        if (stock == null) continue;

    //        results.Add(new
    //        {
    //            Code = stock.Code,
    //            Name = stock.Name,
    //            PredictedReturn = Math.Round(prediction.PredictedValue, 2),
    //            EstimatedPrice = Math.Round(feature.ClosePrice * (1 + prediction.PredictedValue / 100), 2)
    //        });
    //    }

    //    return results
    //        .OrderByDescending(x => (double)x.GetType().GetProperty("PredictedReturn")!.GetValue(x)!)
    //        .Take(count)
    //        .ToList();
    //}
}