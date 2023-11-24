using SplitBackApi.Api.Endpoints.OpenAI.Requests;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Api.Services;

namespace SplitBackApi.Api.Endpoints.OpenAI;
public static partial class OpenAIEndpoints
{

  private static async Task<IResult> Explanator2(
   ChatAndTextService2 chatAndTextService,
   OpenAITextRequest request,
   IGroupRepository groupRepository,
   OpenAIService openAIService
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

    var summaryResult = await openAIService.GetSummarisedText(concatenatedExplanationText);
    if (summaryResult.IsFailure) return Results.BadRequest("Could not get summary from openAI");

    var summary = summaryResult.Value;

    return Results.Ok(summary);
  }
}
