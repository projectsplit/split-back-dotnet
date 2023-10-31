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
  private static async Task<IResult> GetGroupsTotalAmounts2(
      ClaimsPrincipal claimsPrincipal,
      IGroupRepository groupRepository,
      HttpRequest request,
      TransactionService transactionService)
  {
    var authenticatedUserId = "63ff33b09e4437f07d9d3982";//claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var userIsReceiverTotal = new Dictionary<string, decimal>();
    var userIsSenderTotal = new Dictionary<string, decimal>();

    foreach (var group in groups)
    {
      var pendingResult = await transactionService.PendingTransactionsAsync(group.Id);
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
      if (aggregatedSummary.userIsSenderTotal.TryGetValue(currency, out decimal userOwesAmount))
      {
        var userIsOwedAmount = aggregatedSummary.userIsReceiverTotal[currency];

        // Calculate the difference
        var difference = Math.Abs(userOwesAmount - userIsOwedAmount);
        var maxAmount = Math.Max(userOwesAmount, userIsOwedAmount);

        // Update the summaries
        switch (userOwesAmount - userIsOwedAmount)
        {
          case > 0:
            aggregatedSummary.userIsSenderTotal[currency] = difference;
            aggregatedSummary.userIsReceiverTotal[currency] = 0;
            break;
          case < 0:
            aggregatedSummary.userIsSenderTotal[currency] = 0;
            aggregatedSummary.userIsReceiverTotal[currency] = difference;
            break;
          default: // Both amounts are equal
            aggregatedSummary.userIsSenderTotal[currency] = 0;
            aggregatedSummary.userIsReceiverTotal[currency] = 0;
            break;
        }

        // Check if the amount is zero, then add to the list to remove the entry
        if (aggregatedSummary.userIsSenderTotal[currency] == 0) currenciesToRemoveUserIsSender.Add(currency);

        if (aggregatedSummary.userIsReceiverTotal[currency] == 0) currenciesToRemoveUserIsReceiver.Add(currency);
      }
    });


    currenciesToRemoveUserIsSender.Select(aggregatedSummary.userIsSenderTotal.Remove);
    currenciesToRemoveUserIsReceiver.Select(aggregatedSummary.userIsReceiverTotal.Remove);

    var aggregatedSummaryResponse = new GroupsAggregatedSummaryResponse
    {
      UserIsOwedAmounts = aggregatedSummary.userIsReceiverTotal,
      UserOwesAmounts = aggregatedSummary.userIsSenderTotal,
      numberOfGroups = groups.Count
    };

    return Results.Ok(aggregatedSummaryResponse);
  }
}