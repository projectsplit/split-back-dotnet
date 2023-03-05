using FluentValidation;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Validators;

public class CommentValidator : AbstractValidator<Comment> {

  public CommentValidator() {
    
    RuleFor(c => c.Text)
      .MinimumLength(1)
      .MaximumLength(280)
      .WithMessage("Comment text must not exceed 280 characters");

    RuleFor(c => c.MemberId)
      .NotEmpty()
      .WithMessage("Member Id is required");
      
    RuleFor(c => c.ParentId)
      .NotEmpty()
      .WithMessage("Parent Id is required");
  }
}