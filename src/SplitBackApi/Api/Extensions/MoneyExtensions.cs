using System.Security.Claims;
using NMoneys;

namespace SplitBackApi.Api.Extensions;

public static class MoneyExtensions {

  public static CurrencyIsoCode StringToIsoCode(this string currency) {

    return  Enum.Parse<CurrencyIsoCode>(currency);
  }
}