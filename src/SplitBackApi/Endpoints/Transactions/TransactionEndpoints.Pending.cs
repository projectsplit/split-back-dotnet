using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Services;
using System.Security.Claims;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransactionEndpoints {

  private static async Task<IResult> Pending(
    ClaimsPrincipal claimsPrincipal,
    TransactionService transactionService,
    IGroupRepository groupRepository,
    PendingTransactionsRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"User with id {authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    var pendingResult = await transactionService.PendingTransactionsAsync(group.Id);
    if(pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
    
    return Results.Ok(pendingResult.Value);
  }
}