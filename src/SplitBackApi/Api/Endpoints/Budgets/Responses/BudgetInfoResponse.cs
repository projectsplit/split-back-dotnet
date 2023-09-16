using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Budgets.Responses;

public class BudgetInfoResponse
{
  public string TotalAmountSpent { get; set; }
  public string RemainingDays { get; set; }
  public string AverageSpentPerDay { get; set; }
  public string Goal { get; set; }
  public string Currency { get; set; }
  public BudgetType BudgetType {get;set;}
  public string Day {get;set;}

}