using CSharpFunctionalExtensions;
using SplitBackApi.Api.Endpoints.OpenAI.Response;
using SplitBackApi.Api.Services.HttpClients;

namespace SplitBackApi.Api.Services;

public class OpenAIService
{
  private readonly OpenAIClient _opeanAIClient;

  public OpenAIService(OpenAIClient openAIClient)
  {
    _opeanAIClient = openAIClient;
  }

  public async Task<Result<OpenAIExplanatorResponse>> GetSummarisedText(string deterministicExplanationText)
  {
    return await _opeanAIClient.GetSummarisedTextForAllCurrencies(deterministicExplanationText);
  }

  public async Task<Result<OpenAIChatCompletionResponse>> GetChatMessage(string script, string content)
  {
    return await _opeanAIClient.GetChatResponse(script, content);
  }
}