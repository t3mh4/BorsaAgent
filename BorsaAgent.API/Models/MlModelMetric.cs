namespace BorsaAgent.API.Models;

public class MlModelMetric
{
    public int Id { get; set; }
    public double RSquared { get; set; }
    public double Rmse { get; set; }
    public double Mae { get; set; }
    public int TrainingDataCount { get; set; }
    public string ModelPath { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } // "RSI eklendi", "SMA20 kaldırıldı" gibi
}