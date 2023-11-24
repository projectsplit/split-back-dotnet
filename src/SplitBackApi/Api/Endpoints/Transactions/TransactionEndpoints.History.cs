using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transactions.Requests;
using SplitBackApi.Api.Endpoints.Transactions.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.Transactions;

public static partial class TransactionEndpoints
{

  private static async Task<IResult> History(
   ClaimsPrincipal claimsPrincipal,
   IGroupRepository groupRepository,
   TransactionService2 transactionService,
   TransactionHistoryRequest request
  )
  {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groupResult = await groupRepository.GetById(request.GroupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var member = group.GetMemberByUserId(authenticatedUserId);
    if (member is null) return Results.BadRequest("Member not in group");

    var historyResult = await transactionService.GetTransactionHistory(group.Id, member.MemberId);
    if (historyResult.IsFailure) return Results.BadRequest(historyResult.Error);

    var history = historyResult.Value;

    var response = history.ToDictionary(
        h => h.Key,
        h => h.Value.Select(item => new TransactionTimelineItemWithDecimals
        {
          Id = item.Id,
          TransactionTime = item.TransactionTime,
          Description = item.Description,
          Lent = item.Lent.Amount,
          Borrowed = item.Borrowed.Amount,
          UserPaid = item.UserPaid.Amount,
          UserShare = item.UserShare.Amount,
          IsTransfer = item.IsTransfer,
          TotalLent = item.TotalLent.Amount,
          TotalBorrowed = item.TotalBorrowed.Amount,
          Balance = item.Balance.Amount,
          Currency = item.Currency
        }).ToList()
         );

    return Results.Ok(response);
  }
}