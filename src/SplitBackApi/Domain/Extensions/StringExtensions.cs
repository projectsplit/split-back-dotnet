namespace SplitBackApi.Domain.Extensions;

public static class StringExtensions {

  public static decimal ToDecimal(this string str) {

    bool successfullyParsed = decimal.TryParse(str, out decimal x);

    if(successfullyParsed) {
      return x;
    } else {
      throw new Exception();
    }
  }

  public static int ToInt(this string str) {

    bool successfullyParsed = int.TryParse(str, out int x);

    if(successfullyParsed) {
      return x;
    } else {
      throw new Exception();
    }
  }

  public static bool IsDecimal(this string str) {

    return decimal.TryParse(str, out decimal x);
  }

  public static bool HasNoMoreThanTwoDecimalPlaces(this Decimal input) {

    return Decimal.Round(input, 2) == input;
  }
  
}