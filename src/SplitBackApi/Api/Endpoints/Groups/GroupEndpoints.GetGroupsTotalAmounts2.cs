using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Endpoints.Groups.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Domain.Models;
using NMoneys;

// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints
{
  private static async Task<IResult> GetGroupsTotalAmounts2(
      ClaimsPrincipal claimsPrincipal,
      IGroupRepository groupRepository,
      HttpRequest request,
      TransactionService2 transactionService)
  {
    var authenticatedUserId =claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var userIsReceiverTotal = new Dictionary<string, Money>();
    var userIsSenderTotal = new Dictionary<string, Money>();

    var pendingTransactionsResult = await transactionService.PendingTransactionsForAllGroupsAsync(groups);
    if (pendingTransactionsResult.IsFailure) return Results.BadRequest(pendingTransactionsResult.Error);
    var pendingTransactions = pendingTransactionsResult.Value;

    pendingTransactions.ForEach(pendingTransaction =>
  {
    ProcessPendingTransactionBasedOnUserIsSenderUserIsReceiver(
      groups.SelectMany(g => g.Members).ToList(),
      authenticatedUserId,
      userIsReceiverTotal,
      userIsSenderTotal,
      pendingTransaction);
  });

    var aggregatedSummary = new
    {
      userIsReceiverTotal,
      userIsSenderTotal
    };

    var currenciesToRemoveUserIsSender = new List<string>();
    var currenciesToRemoveUserIsReceiver = new List<string>();

    UpdateAggregatedSummaries(
      aggregatedSummary.userIsSenderTotal,
      aggregatedSummary.userIsReceiverTotal,
      currenciesToRemoveUserIsSender,
      currenciesToRemoveUserIsReceiver);

    var aggregatedSummaryResponse = new GroupsAggregatedSummaryResponse
    {
      UserIsOwedAmounts = aggregatedSummary.userIsReceiverTotal.Where(kvp => kvp.Value.Amount != 0).ToDictionary(
      kvp => kvp.Key,
      kvp => kvp.Value.Amount
    ),
      UserOwesAmounts = aggregatedSummary.userIsSenderTotal.Where(kvp => kvp.Value.Amount != 0).ToDictionary(
      kvp => kvp.Key,
      kvp => kvp.Value.Amount
    ),
      NumberOfGroups = groups.Count
    };

    return Results.Ok(aggregatedSummaryResponse);
  }

  private static void UpdateTotals(Dictionary<string, Money> totals, string currency, Money amount)
  {
    totals[currency] = totals.ContainsKey(currency) switch
    {
      true => totals[currency] = totals[currency].Plus(amount),
      _ => totals[currency] = amount
    };
  }

  private static void UpdateAggregatedSummaries(
    Dictionary<string, Money> userIsSenderTotal,
    Dictionary<string, Money> userIsReceiverTotal,
    List<string> currenciesToRemoveUserIsSender,
    List<string> currenciesToRemoveUserIsReceiver)
  {
    foreach (var currency in userIsReceiverTotal.Keys.ToList())
    {
      if (userIsSenderTotal.TryGetValue(currency, out Money userOwesAmount))
      {
        var userIsOwedAmount = userIsReceiverTotal[currency];
        var currencyIso = currency.StringToIsoCode();

        // Calculate the difference
        var difference = Math.Abs((userOwesAmount - userIsOwedAmount).Amount);

        // Update the summaries
        switch (userOwesAmount.CompareTo(userIsOwedAmount))
        {
          case > 0:
            userIsSenderTotal[currency] = new Money(difference, currencyIso);
            userIsReceiverTotal[currency] = Money.Zero(currencyIso);
            break;
          case < 0:
            userIsSenderTotal[currency] = Money.Zero(currencyIso);
            userIsReceiverTotal[currency] = new Money(difference, currencyIso);
            break;
          default: // Both amounts are equal
            userIsSenderTotal[currency] = Money.Zero(currencyIso);
            userIsReceiverTotal[currency] = Money.Zero(currencyIso);
            break;
        }

        // Check if the amount is zero, then add to the list to remove the entry
        if (userIsSenderTotal[currency] == Money.Zero(currencyIso)) currenciesToRemoveUserIsSender.Add(currency);
        if (userIsReceiverTotal[currency] == Money.Zero(currencyIso)) currenciesToRemoveUserIsReceiver.Add(currency);
      }
    }

    // Remove currencies marked for removal
    currenciesToRemoveUserIsSender.ForEach(currency => userIsSenderTotal.Remove(currency));
    currenciesToRemoveUserIsReceiver.ForEach(currency => userIsReceiverTotal.Remove(currency));
  }

  private static void ProcessPendingTransactionBasedOnUserIsSenderUserIsReceiver(
      List<Member> members,
      string authenticatedUserId,
      Dictionary<string, Money> userIsReceiverTotal,
      Dictionary<string, Money> userIsSenderTotal,
      PendingTransaction2 pendingTransaction)
  {
    var userIsReceiver =
        members.Any(m =>
        m is UserMember member &&
        member.MemberId == pendingTransaction.ReceiverId &&
        member.UserId == authenticatedUserId);

    var userIsSender =
        members.Any(m =>
        m is UserMember member &&
        member.MemberId == pendingTransaction.SenderId &&
        member.UserId == authenticatedUserId);

    if (userIsReceiver) UpdateTotals(userIsReceiverTotal, pendingTransaction.Currency, pendingTransaction.Amount);
    if (userIsSender) UpdateTotals(userIsSenderTotal, pendingTransaction.Currency, pendingTransaction.Amount);

  }
}