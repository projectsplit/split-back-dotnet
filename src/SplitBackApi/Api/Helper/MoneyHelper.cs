namespace SplitBackApi.Api.Helper;
using NMoneys;

public class MoneyHelper
{
  public static CurrencyIsoCode StringToIsoCode(string currency)
  {
    // TODO make extention
    return Enum.Parse<CurrencyIsoCode>(currency);
  }

}