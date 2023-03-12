using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Api.Endpoints.OpenAI.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.OpenAI;

public static partial class OpenAIEndpoints {

  private static async Task<IResult> Chat(
   TransactionService transactionService,
   OpenAITextRequest request,
   IGroupRepository groupRepository,
   IOptions<AppSettings> appSettings
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var textResult = await transactionService.GenerateTransactionsExplanationTextAsync(group.Id, request.MemberId);
    if(textResult.IsFailure) return Results.BadRequest(textResult.Error);
    var explanationText = textResult.Value;
    string concatenatedExplanationText = string.Join("  ", explanationText.Select(e => e.Txt));

    var secretKey = appSettings.Value.OpenAI.SecretKey;

    using var client = new HttpClient();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

    var openAIModelrequest = new OpenAIModelRequest {
      model = "text-davinci-003",
      prompt = $"Explain the following text in an easier to understand manner. Please do not use gender specific words: {concatenatedExplanationText}",
      max_tokens = 300,
      temperature = 0.05,
      top_p = 0.05
    };

    var serializedRequest = JsonConvert.SerializeObject(openAIModelrequest);
    var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
    var responseString = await response.Content.ReadAsStringAsync();

    TextCompletionResponse deserializedResponse = JsonConvert.DeserializeObject<TextCompletionResponse>(responseString);

    return Results.Ok(deserializedResponse);
  }
}
