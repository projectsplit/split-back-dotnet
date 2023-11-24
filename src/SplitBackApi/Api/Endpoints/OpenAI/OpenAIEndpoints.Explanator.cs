using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Api.Endpoints.OpenAI.Requests;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Api.Endpoints.OpenAI.Response;

namespace SplitBackApi.Api.Endpoints.OpenAI;

public static partial class OpenAIEndpoints
{

  private static async Task<IResult> Explanator(
   ChatAndTextService2 chatAndTextService,
   OpenAITextRequest request,
   IGroupRepository groupRepository,
   IOptions<AppSettings> appSettings
  )
  {

    if (string.IsNullOrEmpty(request.GroupId)) return Results.BadRequest("No groupId was provided");
    if (string.IsNullOrEmpty(request.MemberId)) return Results.BadRequest("No memberId was provided");

    var groupResult = await groupRepository.GetById(request.GroupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var textResult = await chatAndTextService.GenerateTransactionsExplanationTextAsync(group.Id);

    if (textResult.IsFailure) return Results.BadRequest(textResult.Error);
    var explanationText = textResult.Value;
    string concatenatedExplanationText = string.Join("  ", explanationText.Select(e => e.Txt));

    var secretKey = appSettings.Value.OpenAI.SecretKey;

    using var client = new HttpClient();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

    var openAIDavinciRequest = new OpenAIDavinciRequest
    {
      Model = "text-davinci-003",
      Prompt = $"{concatenatedExplanationText} Rephrase the text above in an easy to understand manner for each currency individually.",
      Max_tokens = 1000,
      Temperature = 0.1,
      Top_p = 0.1,
      Frequency_penalty = 0.2,
      Presence_penalty = 0.2,
      Best_of = 1
    };

    var serializedRequest = JsonConvert.SerializeObject(openAIDavinciRequest);
    var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
    var responseString = await response.Content.ReadAsStringAsync();

    var deserializedResponse = JsonConvert.DeserializeObject<OpenAIExplanatorResponse>(responseString);

    return Results.Ok(deserializedResponse);
  }
}
