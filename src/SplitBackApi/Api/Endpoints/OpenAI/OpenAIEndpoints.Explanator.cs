using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Api.Endpoints.OpenAI.Requests;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.OpenAI;

public static partial class OpenAIEndpoints {

  private static async Task<IResult> Explanator(
   OpenAIService openAIService,
   OpenAITextRequest request,
   IGroupRepository groupRepository,
   IOptions<AppSettings> appSettings
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var textResult = await openAIService.GenerateTransactionsExplanationTextAsync(group.Id);

    if(textResult.IsFailure) return Results.BadRequest(textResult.Error);
    var explanationText = textResult.Value;
    string concatenatedExplanationText = string.Join("  ", explanationText.Select(e => e.Txt));

    var secretKey = appSettings.Value.OpenAI.SecretKey;

    using var client = new HttpClient();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

    var openAIDavinciRequest = new OpenAIDavinciRequest {
      model = "text-davinci-003",
      prompt = $"{concatenatedExplanationText} Rephrase the text above in an easy to understand manner for each currency individually.",
      max_tokens = 1000,
      temperature = 0.1,
      top_p = 0.1,
      frequency_penalty = 0.2,
      presence_penalty = 0.2,
      best_of = 1
    };
    
    var serializedRequest = JsonConvert.SerializeObject(openAIDavinciRequest);
    var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
    var responseString = await response.Content.ReadAsStringAsync();

    var deserializedResponse = JsonConvert.DeserializeObject<ExplanatorResponse>(responseString);

    return Results.Ok(deserializedResponse);
  }
}
