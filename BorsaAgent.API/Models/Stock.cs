namespace BorsaAgent.API.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public string Code { get; set; }      // DOAS.IS
        public string ShortCode { get; set; } // DOAS
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DailyPrice> DailyPrices { get; set; }
        public ICollection<StockSummary> Summaries { get; set; } 
    }
}
