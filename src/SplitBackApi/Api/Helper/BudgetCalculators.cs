using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Extensions;
using System.Globalization;

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

  public static Result<int> RemainingDays(BudgetType budgetType, DateTime startDate)
  {
    DateTime currentDate = DateTime.Now;
    int remainingDays;

    switch (budgetType)
    {
      case BudgetType.Monthly:

        remainingDays = (startDate.AddMonths(1) - currentDate).Days + 1;
        //var x  = (startDate.AddMonths(1) - currentDate);
        break;

      case BudgetType.Weekly:

        remainingDays = (startDate.AddDays(7) - currentDate).Days + 1;

        break;

      default:
        throw new NotImplementedException("Unsupported budget type");
    }
    return Result.Success(remainingDays);

  }

}


