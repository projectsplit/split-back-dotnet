using FluentValidation;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class UserMemberValidator : AbstractValidator<UserMember> {

  public UserMemberValidator() {
    
    RuleFor(u => u.UserId)
      .NotEmpty()
      .WithMessage("User Id is required");

    RuleFor(u => u.MemberId)
      .NotEmpty()
      .WithMessage("Member Id is required");

    RuleFor(u => u.Permissions)
      .IsInEnum()
      .WithMessage("Invalid permissions");
  }
}