using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Endpoints.Groups.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Domain.Models;
using NMoneys;
using SplitBackApi.Api.Helper;
using ZstdSharp.Unsafe;

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
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var userIsReceiverTotal = new Dictionary<string, Money>();
    var userIsSenderTotal = new Dictionary<string, Money>();

    foreach (var group in groups)
    {
      var pendingResult = await transactionService.PendingTransactionsAsync2(group.Id);
      if (pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
      var pendingTransactions = pendingResult.Value;

      pendingTransactions.ForEach(pendingTransaction =>
      {
        var userIsReceiver =
        group.Members.Any(m =>
            m is UserMember member &&
            member.MemberId == pendingTransaction.ReceiverId &&
            member.UserId == authenticatedUserId);

        var userIsSender =
        group.Members.Any(m =>
            m is UserMember member &&
            member.MemberId == pendingTransaction.SenderId &&
            member.UserId == authenticatedUserId);
            
        switch (userIsReceiver, userIsSender)
        {
          case (true, false):
            userIsReceiverTotal[pendingTransaction.Currency] = userIsReceiverTotal.ContainsKey(pendingTransaction.Currency) switch
            {
              true => userIsReceiverTotal[pendingTransaction.Currency].Plus(pendingTransaction.Amount),
              _ => userIsReceiverTotal[pendingTransaction.Currency] = pendingTransaction.Amount
            };
            break;

          case (false, true):
            userIsSenderTotal[pendingTransaction.Currency] = userIsSenderTotal.ContainsKey(pendingTransaction.Currency) switch
            {
              true => userIsSenderTotal[pendingTransaction.Currency] = userIsSenderTotal[pendingTransaction.Currency].Plus(pendingTransaction.Amount),
              _ => userIsSenderTotal[pendingTransaction.Currency] = pendingTransaction.Amount
            };
            break;
        }
      });
    }

    var aggregatedSummary = new
    {
      userIsReceiverTotal,
      userIsSenderTotal
    };

    var currenciesToRemoveUserIsSender = new List<string>();
    var currenciesToRemoveUserIsReceiver = new List<string>();

    aggregatedSummary.userIsReceiverTotal.Keys.ToList().ForEach(currency =>
    {
      if (aggregatedSummary.userIsSenderTotal.TryGetValue(currency, out Money userOwesAmount))
      {
        var userIsOwedAmount = aggregatedSummary.userIsReceiverTotal[currency];
        var currencyIso = MoneyHelper.StringToIsoCode(currency);

        // Calculate the difference
        var difference = Math.Abs((userOwesAmount - userIsOwedAmount).Amount);

        // Update the summaries
        switch (userOwesAmount.CompareTo(userIsOwedAmount))
        {
          case > 0:
            aggregatedSummary.userIsSenderTotal[currency] = new Money(difference, currencyIso);
            aggregatedSummary.userIsReceiverTotal[currency] = Money.Zero(currencyIso);
            break;
          case < 0:
            aggregatedSummary.userIsSenderTotal[currency] = Money.Zero(currencyIso);
            aggregatedSummary.userIsReceiverTotal[currency] = new Money(difference, currencyIso);
            break;
          default: // Both amounts are equal
            aggregatedSummary.userIsSenderTotal[currency] = Money.Zero(currencyIso);
            aggregatedSummary.userIsReceiverTotal[currency] = Money.Zero(currencyIso);
            break;
        }

        // Check if the amount is zero, then add to the list to remove the entry
        if (aggregatedSummary.userIsSenderTotal[currency] == Money.Zero(currencyIso)) currenciesToRemoveUserIsSender.Add(currency);

        if (aggregatedSummary.userIsReceiverTotal[currency] == Money.Zero(currencyIso)) currenciesToRemoveUserIsReceiver.Add(currency);
      }
    });


    currenciesToRemoveUserIsSender.Select(aggregatedSummary.userIsSenderTotal.Remove);
    currenciesToRemoveUserIsReceiver.Select(aggregatedSummary.userIsReceiverTotal.Remove);

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
}