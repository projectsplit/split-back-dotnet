using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Api.Helper;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> GetExpensesByGroup(
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    GetExpensesByGroupRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if(membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var expenses = await expenseRepository.GetByGroupIdPerPage(request.GroupId, request.PageNumber, request.PageSize);


    return Results.Ok(expenses);
  }
}