using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Extensions;


namespace SplitBackApi.Api.Services;

public class BudgetService
{
  public Result<(DateTime startDate, DateTime endDate)> StartAndEndDateBasedOnBudgetAndDay(BudgetType budgetType, string day)
  {
    DateTime currentDate = DateTime.Now;
    DateTime startDate;
    DateTime endDate;
    var day2Int = day.ToInt();

    switch (budgetType)
    {
      case BudgetType.Monthly:

        if (currentDate.Day >= day2Int)
        {
          int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
          startDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(day2Int, daysInMonth));
        }
        else
        {
          // Calculate the previous month
          DateTime previousMonth = currentDate.AddMonths(-1);

          if (currentDate.Month == 1) //check if January so it will go to Dec of prev year
          {
            // If we are in January, go back to December of the previous year
            int daysInMonth = DateTime.DaysInMonth(previousMonth.Year - 1, 12);
            startDate = new DateTime(previousMonth.Year - 1, 12, Math.Min(day2Int, daysInMonth));
          }
          else
          {
            // Go back to the previous month of the current year
            int daysInMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            startDate = new DateTime(previousMonth.Year, previousMonth.Month, Math.Min(day2Int, daysInMonth));
          }
        }

        var tempEndDate = startDate.AddMonths(1);
        //case where previous month has 30 and next month 31 days.
        //February does not have an issue as regardless at what day the previous month ends
        //adding a month to it will alwasy bring to the 28 or 29 of Feb.
        endDate = (tempEndDate.Day == 30 && day2Int == 31) ? tempEndDate.AddDays(1) : tempEndDate;

        if (endDate < currentDate)
        {
          startDate = endDate.Date;
          endDate = startDate.AddMonths(1);
        };

        break;

      case BudgetType.Weekly:
        int dayDifference = (int)currentDate.DayOfWeek - day2Int;

        if (dayDifference >= 0)
        {
          startDate = currentDate.AddDays(-dayDifference);
        }
        else
        {
          // Start from the day2Int of the previous week
          startDate = currentDate.AddDays(-dayDifference - 7);
        }
        startDate = startDate.Date;
        endDate = startDate.AddDays(7);
        break;


      default:
        throw new NotImplementedException("Unsupported budget type");
    }

    return Result.Success((startDate, endDate));
  }

  public Result<double> RemainingDays(BudgetType budgetType, DateTime startDate)
  {
    DateTime currentDate = DateTime.Now;
    double remainingDays;

    switch (budgetType)
    {
      case BudgetType.Monthly:

        remainingDays = (startDate.AddMonths(1) - currentDate).TotalDays;
        var x = startDate.AddMonths(1);
        break;

      case BudgetType.Weekly:

        remainingDays = (startDate.AddDays(7) - currentDate).TotalDays;

        break;

      default:
        throw new NotImplementedException("Unsupported budget type");
    }
    return Result.Success(remainingDays);

  }
}