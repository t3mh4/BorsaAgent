namespace BorsaAgent.API.Infrastructure.Database.Models;

/// <summary>
/// Hisse bilgisi
/// </summary>
public class Stock
{
    public int Id { get; set; }
    public string Symbol { get; set; }      // DOAS, ASELS, vb.
    public string Name { get; set; }        // Doğan Şirketler Grubu, vb.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<StockDailyData> DailyData { get; set; } = new List<StockDailyData>();
    public ICollection<MidasSnapshot> MidasSnapshots { get; set; } = new List<MidasSnapshot>();
}