namespace BorsaAgent.API.Features.DataCollector;

public class TwelveDataResponse
{
    public List<TwelveDataStock> Data { get; set; } = [];
}

public class TwelveDataStock
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class YahooFinanceResponse
{
    public YahooChart Chart { get; set; }
}

public class YahooChart
{
    public List<YahooResult> Result { get; set; }
    public YahooError Error { get; set; }
}

public class YahooResult
{
    public List<long> Timestamp { get; set; }
    public YahooIndicators Indicators { get; set; }
}

public class YahooIndicators
{
    public List<YahooQuote> Quote { get; set; }
}

public class YahooQuote
{
    public List<decimal?> Open { get; set; }
    public List<decimal?> High { get; set; }
    public List<decimal?> Low { get; set; }
    public List<decimal?> Close { get; set; }
    public List<long?> Volume { get; set; }
}

public class YahooError
{
    public string Code { get; set; }
    public string Description { get; set; }
}