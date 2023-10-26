using FluentValidation;
using SplitBackApi.Api.Endpoints.Authentication.Requests;

namespace SplitBackApi.Domain.Validators;

public class EmailInitiateValidator : AbstractValidator<EmailInitiateRequest> {
  
  public EmailInitiateValidator() {
    RuleFor(r => r.Email)
      .Cascade(CascadeMode.Stop) // Stop the validation process after the first failure
      .NotEmpty()
      .WithMessage("Email address is required")
      .EmailAddress()
      .WithMessage("Invalid email address format");
  }
};

public class SignInValidator : AbstractValidator<RequestSignInRequest> {
  
  public SignInValidator() {
    RuleFor(r => r.Email)
      .Cascade(CascadeMode.Stop) // Stop the validation process after the first failure
      .NotEmpty()
      .WithMessage("Email address is required")
      .EmailAddress()
      .WithMessage("Invalid email address format");
  }
};

public class SignUpValidator : AbstractValidator<RequestSignUpRequest> {

  public SignUpValidator() {
    RuleFor(r => r.Email)
    .Cascade(CascadeMode.Stop)
    .NotEmpty()
    .WithMessage("Email is required")
    .EmailAddress()
    .WithMessage("Invalid email address format");

    RuleFor(r => r.Nickname)
    .Cascade(CascadeMode.Stop)
    .NotEmpty()
    .WithMessage("A name is required")
    .MinimumLength(3)
    .WithMessage("Name must be at least 3 characters long")
    .MaximumLength(15)
    .WithMessage("Name must not exceed 15 characters");
  }
};