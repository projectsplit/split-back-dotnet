using FluentValidation;
using SplitBackApi.Extensions;
using SplitBackApi.Requests;
using NMoneys;

//Probably need to validate max length of amounts
public class TransferValidator : AbstractValidator<ITransferDto>
{
  public TransferValidator()
  {
    RuleFor(newTransfer => newTransfer.IsoCode)
    .IsEnumName(typeof(CurrencyIsoCode))
    .WithMessage("Invalid Currency");

    RuleFor(newTransfer => newTransfer.Amount)
    .Must(x => decimal.TryParse(x, out var val))
    .WithMessage("A valid amount is required")
    .DependentRules(() =>
    {
      RuleFor(newTransfer => newTransfer.Amount)
    .Must(Amount => Amount.ToDecimal() > 0)
    .WithMessage("Amount cannot be negative or zero");

    RuleFor(newTransfer => newTransfer.SenderId)
    .NotNull().NotEmpty()
    .WithMessage("A sender should be selected");

    RuleFor(newTransfer => newTransfer.ReceiverId)
    .NotNull().NotEmpty()
    .WithMessage("A recipient should be selected");

    RuleFor(newTransfer => newTransfer)
    .Must(nt => nt.ReceiverId != nt.SenderId)
    .WithMessage("You can\'t transfer to the same account");
    });
  }
}