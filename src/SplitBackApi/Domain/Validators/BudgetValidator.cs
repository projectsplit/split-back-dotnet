using FluentValidation;
using NMoneys;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class BudgetValidator : AbstractValidator<Budget>
{
  public BudgetValidator()
  {
    RuleFor(budget => budget.Currency)
    .IsEnumName(typeof(CurrencyIsoCode))
    .WithMessage("Invalid Currency");

    RuleFor(budget => budget.Amount)
    .NotEmpty()
    .WithMessage("Amount is required")
    .Must(x => decimal.TryParse(x, out var val) && x.ToDecimal() > 0)
    .WithMessage("Amount must be a positive number")
    .DependentRules(() =>
    {
      RuleFor(budget => budget.Amount)
      .Must(x => decimal.Round(x.ToDecimal(), 2) == x.ToDecimal())
      .WithMessage("Amount cannot have more than two decimal places");
    });

    RuleFor(budget => budget.Day)
    .NotEmpty()
    .WithMessage("A day should be selected");

    RuleFor(budget => budget.BudgetType)
    .Must(budgetType => Enum.IsDefined(typeof(BudgetType), budgetType))
    .WithMessage("A valid budget type must be selected");

    When(budget => !string.IsNullOrEmpty(budget.Day) && Enum.IsDefined(typeof(BudgetType), budget.BudgetType), () =>
  {
    RuleFor(budget => budget)
    .Custom((budget, context) =>
    {
      switch (budget.BudgetType)
      {
        case BudgetType.Weekly when budget.Day.ToDecimal() > 7:
          context.AddFailure("Day cannot be greater than 7 for a weekly budget.");
          break;

        case BudgetType.Monthly when budget.Day.ToDecimal() > 31:
          context.AddFailure("Day cannot be greater than 31 for a monthly budget.");

          break;
        default:
          break;
      }
    });
  });
  }

}