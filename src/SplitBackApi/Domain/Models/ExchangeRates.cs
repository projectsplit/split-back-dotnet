namespace SplitBackApi.Domain.Models;
public class ExchangeRates : EntityBase
{
  public string Date { get; set; }
  public string Base { get; set; }
  public Dictionary<string, decimal> Rates { get; set; }
}
