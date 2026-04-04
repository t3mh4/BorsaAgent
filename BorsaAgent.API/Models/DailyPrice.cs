namespace BorsaAgent.API.Models
{
    public class DailyPrice
    {
        public long Id { get; set; }
        public int StockId { get; set; }
        public DateOnly TradeDate { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public long Volume { get; set; }
        public decimal? ChangePercent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Stock Stock { get; set; }
    }
}
