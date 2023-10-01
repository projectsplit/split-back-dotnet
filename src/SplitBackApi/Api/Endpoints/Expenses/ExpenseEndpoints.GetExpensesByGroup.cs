using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Api.Helper;
using Microsoft.AspNetCore.Mvc;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints
{
  private static async Task<IResult> GetExpensesByGroup(
    [FromQuery(Name = "groupId")] string groupId,
    [FromQuery(Name = "pageNumber")] int pageNumber,
    [FromQuery(Name = "pageSize")] int pageSize,
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository)
  {
    var groupMaybe = await groupRepository.GetById(groupId);
    if (groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if (membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var expenses = await expenseRepository.GetByGroupIdPerPage(groupId, pageNumber, pageSize);

    var response = expenses.Select(e => new
    {
      e.CreationTime,
      e.Currency,
      e.GroupId,
      e.Description,
      e.Amount,
      e.Payers,
      e.Participants,
      e.ExpenseTime,
      Labels = group.Labels.Where(l => e.Labels.Contains(l.Id))
    }).ToList();

    return Results.Ok(response);
  }
}