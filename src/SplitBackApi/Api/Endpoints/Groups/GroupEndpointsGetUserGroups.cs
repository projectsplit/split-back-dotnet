using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Groups.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Domain.Models;
using SplitBackApi.Api.Models;


// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetUserGroups(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    HttpRequest request,
    TransactionService transactionService
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var limitQuery = request.Query["limit"].ToString();
    if(string.IsNullOrEmpty(limitQuery)) return Results.BadRequest("limit query is missing");
    if(int.TryParse(limitQuery, out var limit) is false) return Results.BadRequest("invalid limit");

    var lastQuery = request.Query["last"].ToString();
    if(string.IsNullOrEmpty(lastQuery)) return Results.BadRequest("last query is missing");
    if(DateTime.TryParse(lastQuery, out var last) is false) return Results.BadRequest("invalid last");

    var groupsResult = await groupRepository.GetPaginatedGroupsByUserId(authenticatedUserId, limit, last);
    if(groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;
    var responseList = new List<GroupsAndPendingTransactionsResponse>();

    foreach(var group in groups) {
      var pendingResult = await transactionService.PendingTransactionsAsync(group.Id);
      if(pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
      var pendingTransactions = pendingResult.Value;

      var authedUserPendingTransactions = pendingTransactions.Select(p => new AuthedUserPendingTransaction {
        Amount = p.Amount,
        Currency = p.Currency,
        UserIsReceiver = group.Members
        .Where(m => m is UserMember && m.MemberId == p.ReceiverId && ((UserMember)m).UserId == authenticatedUserId)
        .Any(),
        UserIsSender = group.Members
        .Where(m => m is UserMember && m.MemberId == p.SenderId && ((UserMember)m).UserId == authenticatedUserId)
        .Any()
      });

      var groupsAndPendingTransactionsResponse = new GroupsAndPendingTransactionsResponse {
        Group = group,
        PendingTransactions = authedUserPendingTransactions
      };
      responseList.Add(groupsAndPendingTransactionsResponse);
    }

    return Results.Ok(responseList);
  }
}