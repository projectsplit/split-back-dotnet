namespace SplitBackApi.Api.Endpoints.Authentication.Requests;

public record SignInResponse(string AccessToken, SessionData SessionData);