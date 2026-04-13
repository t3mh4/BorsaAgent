using BorsaAgent.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Features.MachineLearning;

public class MLBacktestService(IDbContextFactory<AppDbContext> dbFactory, MlPredictionService predictionService)
{
    public async Task<BacktestReport> RunBacktestAsync(int days = 30, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // 1. Son X günün tarihlerini al (eskiden yeniye)
        var availableDates = await db.DailyPrices
            .Select(x => x.TradeDate)
            .Distinct()
            .OrderByDescending(x => x)
            .Take(days + 1)
            .ToListAsync(ct);

        availableDates.Reverse();

        // 2. Capital simulation değişkenleri
        float capital = 100_000f;
        float peakCapital = capital;
        float maxDrawdown = 0f;

        int winCount = 0;
        int lossCount = 0;
        var dayDetails = new List<BacktestDayDetails>();

        // 3. Her gün simülasyonu
        for (int i = 0; i < availableDates.Count - 1; i++)
        {
            DateOnly currentDay = availableDates[i];
            DateOnly nextDay = availableDates[i + 1];

            var picks = await predictionService.GetTopPicksAsync(count: 5, targetDate: currentDay, ct: ct);
            if (!picks.Any()) continue;

            float dayReturnSum = 0f;
            int dayMatches = 0;
            float positionSize = capital / picks.Count;

            foreach (var pick in picks)
            {
                // Gerçek ertesi gün fiyatını DB'den çek
                var actualNextPrice = await db.DailyPrices
                    .Where(x => x.Stock.Code == pick.Code && x.TradeDate == nextDay)
                    .Select(x => (float)x.ClosePrice)
                    .FirstOrDefaultAsync(ct);

                if (actualNextPrice > 0)
                {
                    float tradeReturn = ((actualNextPrice - pick.LastClose) / pick.LastClose) * 100f;
                    dayReturnSum += tradeReturn;

                    // ✅ Compounding: her pozisyon kendi payından kazanıyor
                    float profit = positionSize * (tradeReturn / 100f);
                    capital += profit;

                    if (tradeReturn > 0) winCount++; else lossCount++;
                    dayMatches++;
                }
            }

            // ✅ Max Drawdown hesaplama
            if (capital > peakCapital)
                peakCapital = capital;

            float drawdown = peakCapital > 0 ? (peakCapital - capital) / peakCapital * 100f : 0f;
            if (drawdown > maxDrawdown)
                maxDrawdown = drawdown;

            float dayAvgReturn = dayMatches > 0 ? dayReturnSum / dayMatches : 0f;
            dayDetails.Add(new BacktestDayDetails(currentDay, dayMatches, dayAvgReturn, capital));
        }

        int totalTrades = winCount + lossCount;
        float totalReturn = (capital - 100_000f) / 100_000f * 100f;

        return new BacktestReport(
            InitialCapital: 100_000f,
            FinalCapital: capital,
            TotalReturn: (float)Math.Round(totalReturn, 2),
            MaxDrawdown: (float)Math.Round(maxDrawdown, 2),
            AccuracyRate: totalTrades > 0 ? (float)Math.Round((float)winCount / totalTrades * 100, 2) : 0f,
            TotalTrades: totalTrades,
            AverageReturnPerTrade: totalTrades > 0 ? (float)Math.Round(totalReturn / dayDetails.Count, 2) : 0f,
            Details: dayDetails
        );
    }

    public record BacktestReport(float InitialCapital,
                                 float FinalCapital,
                                 float TotalReturn,
                                 float MaxDrawdown,
                                 float AccuracyRate,
                                 int TotalTrades,
                                 float AverageReturnPerTrade,
                                 List<BacktestDayDetails> Details);

    public record BacktestDayDetails(DateOnly Date, int MatchCount, float AverageDayReturn, float Capital);
}