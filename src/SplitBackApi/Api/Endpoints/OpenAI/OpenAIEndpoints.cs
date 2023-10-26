namespace SplitBackApi.Api.Endpoints.OpenAI;

public static partial class OpenAIEndpoints {

  public static void MapOpenAIEndpoints(this IEndpointRouteBuilder app) {

    var openaiGroup = app.MapGroup("/openai")
      .WithTags("OpenAI")
      .AllowAnonymous();

    openaiGroup.MapPost("/explain", Explanator);
    openaiGroup.MapPost("/chat", Chat);

  }
}
