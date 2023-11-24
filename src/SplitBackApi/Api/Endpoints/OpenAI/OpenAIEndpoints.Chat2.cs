using CSharpFunctionalExtensions;
using SplitBackApi.Api.Endpoints.OpenAI.Requests;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Api.Services;

namespace SplitBackApi.Api.Endpoints.OpenAI;

public static partial class OpenAIEndpoints
{
  private static async Task<Microsoft.AspNetCore.Http.IResult> Chat2(
   ChatAndTextService2 chatAndTextService,
   OpenAIChatUserInputRequest request,
   IGroupRepository groupRepository,
   OpenAIService openAIService
  )
  {
    if (string.IsNullOrEmpty(request.GroupId)) return Results.BadRequest("No groupId was provided");
    if (string.IsNullOrEmpty(request.Content)) return Results.BadRequest("No question from user was asked");

    var groupResult = await groupRepository.GetById(request.GroupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var textResult = await chatAndTextService.GenerateChatScriptAsync(group.Id);

    if (textResult.IsFailure) return Results.BadRequest(textResult.Error);

    var script = textResult.Value;
    string concatenatedScript = string.Join("  ", script.Select(e => e.Txt));

    var chatResponseResult = await openAIService.GetChatMessage(concatenatedScript, request.Content);
    if (chatResponseResult.IsFailure) return Results.BadRequest("Could not fetch response");

    var chatResponse = chatResponseResult.Value;

    return Results.Ok(chatResponse.Choices[0].Message.Content);
  }
}
