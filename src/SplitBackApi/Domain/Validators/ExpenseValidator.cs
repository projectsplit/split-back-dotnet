using FluentValidation;
using NMoneys;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class ExpenseValidator : AbstractValidator<Expense> {

  public ExpenseValidator() {

    RuleFor(expense => expense.Currency)
      .IsEnumName(typeof(CurrencyIsoCode))
      .WithMessage("Invalid Currency");

    RuleFor(expense => expense.Description)
      .NotEmpty()
      .WithMessage("Description is required");

    RuleFor(expense => expense.Participants.Count)
      .GreaterThan(0)
      .WithName("Participants")
      .WithMessage("At least one member should be selected");

    RuleFor(expense => expense.Payers.Count)
      .GreaterThan(0)
      .WithName("Payers")
      .WithMessage("At least one payer should be selected");

    RuleFor(expense => expense.Amount)
      .Must(x => decimal.TryParse(x, out var val) && x.ToDecimal() > 0)
      .WithMessage("Amount must be a positive number")
      .DependentRules(() => {

        RuleFor(expense => expense.Amount)
          .Must(x => Decimal.Round(x.ToDecimal(), 2) == x.ToDecimal())
          .WithMessage("Amount cannot have more than two decimal places");

        When(expense => expense.Participants.Count > 0, () => {
          RuleForEach(expense => expense.Participants)
            .Must(p => p.ParticipationAmount.IsDecimal() && p.ParticipationAmount.ToDecimal() > 0 && p.ParticipationAmount.ToDecimal().HasNoMoreThanTwoDecimalPlaces())
            .WithName("Participants")
            .WithMessage("Amount must be a positive number")
            .DependentRules(() => {

              RuleFor(expense => expense)
                .Must(e => e.Participants.Sum(p => p.ParticipationAmount.ToDecimal()) == e.Amount.ToDecimal())
                .WithName("Participants")
                .WithMessage("Participants\' amounts don\'t add up to total");
            });
        });

        When(expense => expense.Payers.Count > 0, () => {
          RuleForEach(expense => expense.Payers)
            .Must(p => p.PaymentAmount.IsDecimal() && p.PaymentAmount.ToDecimal() > 0 && p.PaymentAmount.ToDecimal().HasNoMoreThanTwoDecimalPlaces())
            .WithName("Payers")
            .WithMessage("Amount must be a positive number")
            .DependentRules(() => {

              RuleFor(expense => expense)
                .Must(e => e.Payers.Sum(p => p.PaymentAmount.ToDecimal()) == e.Amount.ToDecimal())
                .WithName("Payers")
                .WithMessage("Payers\' amounts don\'t add up to total");
            });
        });
      });

    RuleForEach(expense => expense.Labels)
      .NotEmpty()
      .WithMessage("Label Id is required");
  }
}