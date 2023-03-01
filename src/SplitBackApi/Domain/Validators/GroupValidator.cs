using FluentValidation;
using NMoneys;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class GroupValidator : AbstractValidator<Group> {

  public GroupValidator() {

    RuleFor(group => group.BaseCurrency)
      .IsEnumName(typeof(CurrencyIsoCode))
      .WithMessage("Invalid Currency");

    RuleFor(group => group.Title)
      .MinimumLength(1)
      .MaximumLength(20)
      .WithMessage("Title length must be between 1 and 20 characters");
  }
}