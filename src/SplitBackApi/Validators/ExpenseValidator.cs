using FluentValidation;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using NMoneys;

public class ExpenseValidator : AbstractValidator<IExpenseDto> {

  public ExpenseValidator() {

    RuleFor(newExpense => newExpense.IsoCode)
    .IsEnumName(typeof(CurrencyIsoCode))
    .WithMessage("Invalid Currency");

    RuleFor(newExpense => newExpense.Amount)
    .Must(x => decimal.TryParse(x, out var val))
    .WithMessage("A valid amount is required")
    .DependentRules(() => {
      RuleFor(newExpense => newExpense.Amount)
      .Must(Amount => Amount.ToDecimal() > 0)
      .WithMessage("Amount cannot be negative or zero");

      RuleFor(newExpense => newExpense.Description)
      //.Cascade(CascadeMode.Stop)
      .NotNull().NotEmpty().WithMessage("Description is required");

      When(newExpense => !newExpense.SplitEqually && newExpense.ExpenseParticipants.Count >= 1, () => {
        RuleForEach(newExpense => newExpense.ExpenseParticipants)
       .Must(ep => ep.ContributionAmount.CheckIfDecimal())
       .WithMessage("A valid amount is required")
       .DependentRules(() => {
         RuleFor(newExpense => newExpense)
         .Must(ne => ne.ExpenseParticipants
         .Sum(ep => ep.ContributionAmount.ToDecimal()) == ne.Amount.ToDecimal())
         .WithMessage("Member amounts don\'t add up to total");

         // RuleFor(newExpense => newExpense.ExpenseParticipants.Sum(ep => ep.ContributionAmount.ToDecimal()) == newExpense.Amount.ToDecimal())
         // .Must(sum => sum == true)
         // .WithMessage("Member amounts don\'t add up to total");
       });
      });

      When(newExpense => newExpense.ExpenseSpenders.Count >= 1, () => {
        RuleForEach(newExpense => newExpense.ExpenseParticipants)
       .Must(ep => ep.ContributionAmount.CheckIfDecimal())
       .WithMessage("A valid amount is required")
       .DependentRules(() => {
         RuleFor(newExpense => newExpense)
        .Must(ne => ne.ExpenseSpenders
        .Sum(es => es.SpenderAmount.ToDecimal()) == ne.Amount.ToDecimal())
        .WithMessage("Payers\' amounts don\'t add up to total");
       });
      });

      RuleFor(newExpense => newExpense.ExpenseParticipants.Count)
      .GreaterThan(0)
      .WithMessage("At least one member should be selected");

      RuleFor(newExpense => newExpense.ExpenseSpenders.Count)
      .GreaterThan(0)
      .WithMessage("At least one payer should be selected");
    });
  }
}