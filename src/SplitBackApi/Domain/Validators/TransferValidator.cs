using FluentValidation;
using NMoneys;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class TransferValidator : AbstractValidator<Transfer> {

  public TransferValidator() {
      
    RuleFor(transfer => transfer)
      .Must(t => t.ReceiverId != t.SenderId)
      .WithMessage("Send and receiver cannot be the same");

    RuleFor(transfer => transfer.Currency)
      .IsEnumName(typeof(CurrencyIsoCode))
      .WithMessage("Invalid Currency");

    RuleFor(transfer => transfer.ReceiverId)
      .NotEmpty()
      .WithMessage("A receiver should be selected");

    RuleFor(transfer => transfer.SenderId)
      .NotEmpty()
      .WithMessage("A sender should be selected");

    RuleFor(transfer => transfer.Amount)
      .Must(x => decimal.TryParse(x, out var val) && x.ToDecimal() > 0)
      .WithMessage("Amount must be a positive number")
      .DependentRules(() => {
        RuleFor(transfer => transfer.Amount)
          .Must(x => x.ToDecimal().HasNoMoreThanTwoDecimalPlaces())
          .WithMessage("Amount cannot have more than two decimal places");
      });
      
  }
}