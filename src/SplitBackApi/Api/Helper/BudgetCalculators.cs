using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Extensions;
using System.Globalization;
using Newtonsoft.Json;

namespace SplitBackApi.Api.Helper;

public static class BudgetHelpers
{
  public static Result<DateTime> StartDateBasedOnBudgetAndDay(BudgetType budgetType, string day)
  {
    DateTime currentDate = DateTime.Now;
    DateTime startDate;
    var day2Int = day.ToInt();

    switch (budgetType)
    {
      case BudgetType.Monthly:
        if (currentDate.Day >= day2Int)
        {

          startDate = new DateTime(currentDate.Year, currentDate.Month, day2Int);
        }
        else
        {
          // Calculate the previous month
          DateTime previousMonth = currentDate.AddMonths(-1);

          if (currentDate.Month == 1) //check if January so it will go to Dec of prev year
          {
            // If we are in January, go back to December of the previous year
            startDate = new DateTime(previousMonth.Year - 1, 12, day2Int);
          }
          else
          {
            // Go back to the previous month of the current year
            startDate = new DateTime(previousMonth.Year, previousMonth.Month, day2Int);
          }
        }
        break;

      case BudgetType.Weekly:
        int dayDifference = (int)currentDate.DayOfWeek - day2Int;

        if (dayDifference > 0)
        {
          startDate = currentDate.AddDays(-dayDifference);
        }
        else
        {
          // Start from the day2Int of the previous week
          startDate = currentDate.AddDays(-dayDifference - 7);
        }
        break;

      default:
        throw new NotImplementedException("Unsupported budget type");
    }

    return Result.Success(startDate);
  }

  public static Result<double> RemainingDays(BudgetType budgetType, DateTime startDate)
  {
    DateTime currentDate = DateTime.Now;
    double remainingDays;

    switch (budgetType)
    {
      case BudgetType.Monthly:

        remainingDays = (startDate.AddMonths(1) - currentDate).TotalDays;
        
        break;

      case BudgetType.Weekly:

        remainingDays = (startDate.AddDays(7) - currentDate).TotalDays;

        break;

      default:
        throw new NotImplementedException("Unsupported budget type");
    }
    return Result.Success(remainingDays);

  }


public async static Task<Result<ExchangeRateResponse>> HistoricalFxRate(string symbols, string baseCurrency, string date){
     using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("accept", "application/json");

    string apiUrl = $"https://openexchangerates.org/api/historical/{date}.json";
    string appId = "382c2dbb473546f2aa9f558a18c8da29";

    var uriBuilder = new UriBuilder(apiUrl);
    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
    query["app_id"] = appId;
    query["base"] = baseCurrency;
    query["symbols"] = symbols;
    query["show_alternative"] = "false";
    query["prettyprint"] = "false";
    query["date"] = date;
    uriBuilder.Query = query.ToString();
    string url = uriBuilder.ToString();

    var response = await client.GetAsync(url);
    string responseString = await response.Content.ReadAsStringAsync();
    var deserializedResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(responseString);
    
    return Result.Success(deserializedResponse);
    
}
}



