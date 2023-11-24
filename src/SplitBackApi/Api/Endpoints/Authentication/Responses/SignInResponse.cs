using SplitBackApi.Api.Models;

namespace SplitBackApi.Api.Endpoints.Authentication.Requests;

public record SignInResponse(string AccessToken, SessionData SessionData);