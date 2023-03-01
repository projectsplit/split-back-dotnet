using System.Security.Claims;
using CSharpFunctionalExtensions;
using FluentValidation.Results;

namespace SplitBackApi.Extensions;

public static class ValidationResultExtensions {

  public static object ToErrorResponse(this ValidationResult validationResult) {

    return validationResult.Errors.Select(e =>
      new {
        Field = e.PropertyName,
        ErrorMessage = e.ErrorMessage
      }
    );
  }
}