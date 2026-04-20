namespace BorsaAgent.API.Infrastructure.TwelveData;

public class TwelveDataResponse
{
    public List<TwelveDataStockInfo> Data { get; set; } = new();
}

public class TwelveDataStockInfo
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}