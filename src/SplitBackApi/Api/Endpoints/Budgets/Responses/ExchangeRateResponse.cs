public class ExchangeRateResponse
{
    public long Timestamp { get; set; }
    public string Base { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}