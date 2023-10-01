using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transactions.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.Transactions;

public static partial class TransactionEndpoints {

  private static async Task<IResult> History(
   ClaimsPrincipal claimsPrincipal,
   IGroupRepository groupRepository,
   TransactionService transactionService,
   TransactionHistoryRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var groupMaybe = await groupRepository.GetById(request.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest("Member not in group");
    
    var historyResult = await transactionService.GetTransactionHistory(group.Id, member.MemberId);
    if(historyResult.IsFailure) return Results.BadRequest(historyResult.Error);
    
    return Results.Ok(historyResult.Value);
  }
}