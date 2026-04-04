namespace BorsaAgent.API.Models
{
    public class StockSummary
    {
        public long Id { get; set; }
        public int StockId { get; set; }
        public DateOnly SummaryDate { get; set; }
        public string SummaryText { get; set; } = string.Empty; // LLM için metin
        public string QdrantPointId { get; set; }              // Qdrant'taki ID referansı
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Stock Stock { get; set; }
    }
}
