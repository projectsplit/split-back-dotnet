namespace SplitBackApi.Extensions;

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

  public static bool CheckIfDecimal(this string str) {

    bool successfullyParsed = decimal.TryParse(str, out decimal x);

    if(successfullyParsed) {
      return true;
    } else {
      return false;
    }
  }
}