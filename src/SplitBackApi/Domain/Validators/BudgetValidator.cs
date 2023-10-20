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
  }

}