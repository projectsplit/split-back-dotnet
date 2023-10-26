namespace SplitBackApi.Api.Endpoints.Authentication.Models;

public record EmailLinkTokenClaims(string Email, string Unique, bool IsNewUser);