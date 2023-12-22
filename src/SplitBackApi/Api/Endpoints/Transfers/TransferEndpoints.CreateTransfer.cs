using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> CreateTransfer(
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    HttpContext httpContext,
    ClaimsPrincipal claimsPrincipal,
    TransferValidator transferValidator,
    CreateTransferRequest request
  ) {

    var authenticatedUserId ="63ff33b09e4437f07d9d3982";//claimsPrincipal.GetAuthenticatedUserId();

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var groupMemberIds = group.Members.Select(m => m.MemberId).ToList();

    var requestMemberIds = new List<string> { request.ReceiverId, request.SenderId };
    var memberIdsAreValid = requestMemberIds.Intersect(groupMemberIds).Count() == requestMemberIds.Count();
    if(memberIdsAreValid is false) return Results.BadRequest("Sender and/or Receiver Id(s) are not valid");

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    var newTransfer = new Transfer {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      Amount = request.Amount,
      Currency = request.Currency,
      Description = request.Description,
      GroupId = request.GroupId,
      ReceiverId = request.ReceiverId,
      SenderId = request.SenderId,
      TransferTime = request.TransferTime
    };

    var validationResult = transferValidator.Validate(newTransfer);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    await transferRepository.Create(newTransfer);

    return Results.Ok();
  }
}