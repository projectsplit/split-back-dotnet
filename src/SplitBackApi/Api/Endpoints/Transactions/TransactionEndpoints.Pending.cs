using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transactions.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.Transactions;

public static partial class TransactionEndpoints {

  private static async Task<IResult> Pending(
    ClaimsPrincipal claimsPrincipal,
    TransactionService2 transactionService,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    PendingTransactionsRequest request
  ) {

    var authenticatedUserId = "63ff33b09e4437f07d9d3982";//claimsPrincipal.GetAuthenticatedUserId();

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"User with id {authenticatedUserId} is not a member of group with id {request.GroupId}"); //not required? idk

    var pendingResult = await transactionService.PendingTransactionsAsync2(group.Id);
    if(pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
    var pendingTransactions = pendingResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if(membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var response = pendingTransactions.Select(p => new PendingTransactionResponse {
      Amount = p.Amount.Amount,
      Currency = p.Currency,
      ReceiverId = p.ReceiverId,
      SenderId = p.SenderId,
      ReceiverName = membersWithNames.Single(m => m.Id == p.ReceiverId).Name,
      SenderName = membersWithNames.Single(m => m.Id == p.SenderId).Name
    });
    
    return Results.Ok(response);
  }
}