using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Budgets.Requests;

public class  CreateBudgetRequest  {
    public string Amount { get; set; }
    public string Currency { get; set; }
    public BudgetType BudgetType { get; set; }
    public string Day { get; set; }

}