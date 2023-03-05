using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Groups.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GuestEndpoints {

  private static async Task<IResult> RemoveGuest(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    RemoveGuestRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var memberToRemove = group.Members.Where(m => m.MemberId == request.MemberId).FirstOrDefault();
    if(memberToRemove is not GuestMember) return Results.BadRequest($"member with Id {memberToRemove.MemberId} is not a guest");

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var guestRemoverMember = group.GetMemberByUserId(authenticatedUserId);
    if(guestRemoverMember is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(guestRemoverMember.Permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    //test if guest participates in any expense or tranfer and send bad request.
    var groupExpenses = await expenseRepository.GetByGroupId(request.GroupId);

    var guestIsPayer = groupExpenses.Any(e => e.Payers.Any(p => p.MemberId == request.MemberId));
    if(guestIsPayer) return Results.BadRequest($"There is at least one expense where guest {request.MemberId} is a payer");

    var guestIsParticipant = groupExpenses.Any(e => e.Participants.Any(p => p.MemberId == request.MemberId));
    if(guestIsParticipant) return Results.BadRequest($"There is at least one expense where guest {request.MemberId} is a participant");

    var groupTransfers = await transferRepository.GetByGroupId(request.GroupId);

    var guestExistsInTransfer = groupTransfers.Any(t => {
      if(t.SenderId == request.MemberId) {
        return true;
      }
      if(t.ReceiverId == request.MemberId) {
        return true;
      }
      return false;
    });

    if(guestExistsInTransfer) return Results.BadRequest($"There is at least one transfer where guest {request.MemberId} is involved");

    if(memberToRemove is null) return Results.BadRequest($"Member {memberToRemove.MemberId} has not been found");
    group.Members.Remove(memberToRemove);

    var groupUpdateResult = await groupRepository.Update(group);
    if(groupUpdateResult.IsFailure) return Results.BadRequest(groupUpdateResult.Error);

    return Results.Ok($"Guest {request.MemberId} removed succesfully");
  }
}