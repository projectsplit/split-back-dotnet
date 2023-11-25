namespace SplitBackApi.Domain.Extensions;

public static class DecimalExtensions {

  public static bool HasNoMoreThanTwoDecimalPlaces(this decimal input) {

    return decimal.Round(input, 2) == input;
  }
  
}