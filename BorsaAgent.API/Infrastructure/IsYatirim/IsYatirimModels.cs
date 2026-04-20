using System.Text.Json.Serialization;

namespace BorsaAgent.API.Infrastructure.IsYatirim;

public class IsYatirimResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("errorDescription")]
    public string ErrorDescription { get; set; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public List<IsYatirimDailyData> Value { get; set; } = new();
}

public class IsYatirimDailyData
{
    [JsonPropertyName("HGDG_TARIH")]
    public string HgdgTarih { get; set; } = string.Empty;

    [JsonPropertyName("HGDG_KAPANIS")]
    public decimal HgdgKapanis { get; set; }

    [JsonPropertyName("HGDG_AOF")]
    public decimal HgdgAof { get; set; }

    [JsonPropertyName("HGDG_MIN")]
    public decimal HgdgMin { get; set; }

    [JsonPropertyName("HGDG_MAX")]
    public decimal HgdgMax { get; set; }

    [JsonPropertyName("HGDG_HACIM")]
    public decimal HgdgHacim { get; set; }
}