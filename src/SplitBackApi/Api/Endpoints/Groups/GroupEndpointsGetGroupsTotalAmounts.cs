using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Endpoints.Groups.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Domain.Models;

// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints
{
  private static async Task<IResult> GetGroupsTotalAmounts(
      ClaimsPrincipal claimsPrincipal,
      IGroupRepository groupRepository,
      HttpRequest request,
      TransactionService transactionService)
  {
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var userIsReceiverTotal = new Dictionary<string, decimal>();
    var userIsSenderTotal = new Dictionary<string, decimal>();

    foreach (var group in groups)
    {
      var pendingResult = await transactionService.PendingTransactionsAsync(group.Id);
      if (pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
      var pendingTransactions = pendingResult.Value;

      foreach (var pendingTransaction in pendingTransactions)
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

        if (userIsReceiver)
        {
          if (userIsReceiverTotal.ContainsKey(pendingTransaction.Currency))
          {
            userIsReceiverTotal[pendingTransaction.Currency] += pendingTransaction.Amount;
          }
          else
          {
            userIsReceiverTotal[pendingTransaction.Currency] = pendingTransaction.Amount;
          }
        }
        else if (userIsSender)
        {
          if (userIsSenderTotal.ContainsKey(pendingTransaction.Currency))
          {
            userIsSenderTotal[pendingTransaction.Currency] += pendingTransaction.Amount;
          }
          else
          {
            userIsSenderTotal[pendingTransaction.Currency] = pendingTransaction.Amount;
          }
        }
      }
    }

    var aggregatedSummary = new
    {
      userIsReceiverTotal,
      userIsSenderTotal
    };

    var currenciesToRemoveUserIsSender = new List<string>();
    var currenciesToRemoveUserIsReceiver = new List<string>();

    foreach (var currency in aggregatedSummary.userIsReceiverTotal.Keys)
    {
      if (aggregatedSummary.userIsSenderTotal.TryGetValue(currency, out decimal userOwesAmount))
      {
        var userIsOwedAmount = aggregatedSummary.userIsReceiverTotal[currency];

        // Calculate the difference
        var difference = Math.Abs(userOwesAmount - userIsOwedAmount);
       var maxAmount = Math.Max(userOwesAmount, userIsOwedAmount);

        // Update the summaries
        if (userOwesAmount > userIsOwedAmount)
        {
          aggregatedSummary.userIsSenderTotal[currency] = difference;
          aggregatedSummary.userIsReceiverTotal[currency] = 0;
        }
        else if (userOwesAmount < userIsOwedAmount)
        {
          aggregatedSummary.userIsSenderTotal[currency] = 0;
          aggregatedSummary.userIsReceiverTotal[currency] = difference;
        }
        else // Both amounts are equal
        {
          aggregatedSummary.userIsSenderTotal[currency] = 0;
          aggregatedSummary.userIsReceiverTotal[currency] = 0;
        }

        // Check if the amount is zero, then add to the list to remove the entry
        if (aggregatedSummary.userIsSenderTotal[currency] == 0)
          currenciesToRemoveUserIsSender.Add(currency);

        if (aggregatedSummary.userIsReceiverTotal[currency] == 0)
          currenciesToRemoveUserIsReceiver.Add(currency);
      }
    }

    foreach (var currency in currenciesToRemoveUserIsSender)
    {
      aggregatedSummary.userIsSenderTotal.Remove(currency);
    }

    foreach (var currency in currenciesToRemoveUserIsReceiver)
    {
      aggregatedSummary.userIsReceiverTotal.Remove(currency);
    }

    var aggregatedSummaryResponse = new GroupsAggregatedSummaryResponse
    {
      UserIsOwedAmounts = aggregatedSummary.userIsReceiverTotal,
      UserOwesAmounts = aggregatedSummary.userIsSenderTotal,
      NumberOfGroups = groups.Count
    };

    return Results.Ok(aggregatedSummaryResponse);
  }
}