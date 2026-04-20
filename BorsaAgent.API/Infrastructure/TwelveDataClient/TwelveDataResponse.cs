namespace BorsaAgent.API.Infrastructure.TwelveDataClient
{
    /// <summary>
    /// TwelveData API response modelleri
    /// </summary>
    public class TwelveDataResponse
    {
        public List<StockInfo> Data { get; set; } = new();
        public string Status { get; set; }
    }

}
