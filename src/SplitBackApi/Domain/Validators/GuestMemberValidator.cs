using FluentValidation;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class GuestMemberValidator : AbstractValidator<GuestMember> {

  public GuestMemberValidator() {
    
    RuleFor(g => g.Name)
      .MinimumLength(1)
      .MaximumLength(20)
      .WithMessage("Guest name must not exceed 20 characters");

    RuleFor(g => g.MemberId)
      .NotEmpty()
      .WithMessage("Member Id is required");

    // RuleFor(g => g.Permissions)
    //   .IsInEnum()
    //   .WithMessage("Invalid permissions");
  }
}