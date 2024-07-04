using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Groups.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;


namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints
{
  private static async Task<IResult> GetGroupTransactions(
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    ITransferRepository transferRepository,
    HttpRequest request
  )
  {
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groupId = request.Query["groupid"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("Group id is missing");

    var limitQuery = request.Query["limit"].ToString();
    if (string.IsNullOrEmpty(limitQuery)) return Results.BadRequest("limit query is missing");
    if (int.TryParse(limitQuery, out var limit) is false) return Results.BadRequest("invalid limit");

    var lastQuery = request.Query["last"].ToString();
    if (string.IsNullOrEmpty(lastQuery)) return Results.BadRequest("last query is missing");
    if (DateTime.TryParse(lastQuery, out var last) is false) return Results.BadRequest("invalid last");

    var payersIds = request.Query["payersIds"].ToString().Split(',');
    // if (payersIds.All(string.IsNullOrEmpty)) return Results.BadRequest("payersIds is missing");

    var participantsIds = request.Query["participantsIds"].ToString().Split(',');
    // if (participantsIds.All(string.IsNullOrEmpty)) return Results.BadRequest("participantsIds is missing");

    var groupResult = await groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var paginatedExpensesResult = await expenseRepository.GetPaginatedExpensesByGroupId(groupId, limit, last, payersIds,participantsIds);
    if (paginatedExpensesResult.IsFailure) return Results.BadRequest(paginatedExpensesResult.Error);

    var paginatedTransfersResult = await transferRepository.GetPaginatedTransfersByGroupId(groupId, limit, last);
    if (paginatedTransfersResult.IsFailure) return Results.BadRequest(paginatedTransfersResult.Error);

    var expenses = paginatedExpensesResult.Value;
    var transfers = paginatedTransfersResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if (membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var expensesWithMemberNames = expenses.Select(e => new ExpenseWithMemberNames
    {
      Amount = e.Amount,
      CreationTime = e.CreationTime,
      Currency = e.Currency,
      Description = e.Description,
      ExpenseTime = e.ExpenseTime,
      // GroupId = e.GroupId,
      // Id = e.Id,
      Labels = e.Labels,
      LastUpdateTime = e.LastUpdateTime,
      Participants = e.Participants.Select(p => new ParticipantWithName { ParticipationAmount = p.ParticipationAmount, Name = membersWithNames.Single(mn => mn.Id == p.MemberId).Name, UserId = MemberIdHelper.MemberIdToUserId(group, p.MemberId) }).ToList(),
      Payers = e.Payers.Select(p => new PayerWithName { PaymentAmount = p.PaymentAmount, Name = membersWithNames.Single(mn => mn.Id == p.MemberId).Name, UserId = MemberIdHelper.MemberIdToUserId(group, p.MemberId) }).ToList(),
      Id = e.Id

    }).ToList();

    var transfersWithMemberNames = transfers.Select(t => new TransferWithMemberNames
    {
      Amount = t.Amount,
      CreationTime = t.CreationTime,
      Currency = t.Currency,
      Description = t.Description,
      // GroupId = t.GroupId,
      Id = t.Id,
      LastUpdateTime = t.LastUpdateTime,
      TransferTime = t.TransferTime,
      ReceiverName = membersWithNames.Single(mn => mn.Id == t.ReceiverId).Name,
      ReceiverUserId = MemberIdHelper.MemberIdToUserId(group, t.ReceiverId),
      SenderName = membersWithNames.Single(mn => mn.Id == t.SenderId).Name,
      SenderUserId = MemberIdHelper.MemberIdToUserId(group, t.SenderId)
    }).ToList();

    var allTransactions = expensesWithMemberNames
     .Select(e => new GroupAllTransactionsResponse(e))
     .Concat(transfersWithMemberNames
     .Select(t => new GroupAllTransactionsResponse(t)));

    var sortedTransactionWrappers = allTransactions.OrderByDescending(tw => tw.TransactionTime).ToList();

    return Results.Ok(sortedTransactionWrappers);
  }
}