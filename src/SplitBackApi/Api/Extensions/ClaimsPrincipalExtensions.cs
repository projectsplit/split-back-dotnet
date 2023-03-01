using System.Security.Claims;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Extensions;

public static class ClaimsPrincipalExtensions {

  public static string GetAuthenticatedUserId(this ClaimsPrincipal claimsPrincipal) {

    return claimsPrincipal.Claims.Where(c => c.Type == "userId").First().Value;
  }
}