using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using AutoMapper;
using SplitBackApi.Domain;
using SplitBackApi.Services;
using System.Security.Claims;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransactionEndpoints {

  private static async Task<IResult> History(
   ClaimsPrincipal claimsPrincipal,
   IGroupRepository groupRepository,
   TransactionService transactionService,
   TransactionHistoryRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest("Member not in group");
    
    var historyResult = await transactionService.GetTransactionHistory(group.Id, member.MemberId);
    if(historyResult.IsFailure) return Results.BadRequest(historyResult.Error);

    return Results.Ok(historyResult.Value);
  }
}