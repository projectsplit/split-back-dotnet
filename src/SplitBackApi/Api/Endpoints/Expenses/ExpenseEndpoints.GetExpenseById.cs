using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using Microsoft.AspNetCore.Mvc;
using SplitBackApi.Domain.Models;
using SplitBackApi.Api.Models.Responses;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints
{
  private static async Task<IResult> GetExpenseById(
    [FromRoute] string expenseId,
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository)
  {

    // await Task.Delay(2000);

    var expenseMaybe = await expenseRepository.GetById(expenseId);
    if (expenseMaybe.HasNoValue) return Results.BadRequest("Expense not found");
    var expense = expenseMaybe.Value;

    var groupMaybe = await groupRepository.GetById(expense.GroupId);
    if (groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var userMembers = group.Members.Where(m => m is UserMember).Cast<UserMember>();
    var users = await userRepository.GetByIds(userMembers.Select(u => u.UserId).ToList());

    var memberLookup = userMembers.ToDictionary(m => m.MemberId, m => m);
    var userLookup = users.ToDictionary(u => u.Id, u => u);
    var labelLookup = group.Labels.ToDictionary(l => l.Id, l => l);

    var participants = expense.Participants.Select(p => new ParticipantResponse
    {
      MemberId = p.MemberId,
      Amount = p.ParticipationAmount,
      Name = userLookup[memberLookup[p.MemberId].UserId].Nickname
    }).ToList();

    var payers = expense.Payers.Select(p => new PayerResponse
    {
      MemberId = p.MemberId,
      Amount = p.PaymentAmount,
      Name = userLookup[memberLookup[p.MemberId].UserId].Nickname
    }).ToList();

    var labels = expense.Labels.Select(l => new LabelResponse
    {
      Id = l,
      Text = labelLookup[l].Text,
      Color = labelLookup[l].Color,
    }).ToList();

    var response = new GetExpenseResponse
    {
      Id = expense.Id,
      GroupId = expense.GroupId,
      Amount = expense.Amount,
      Currency = expense.Currency,
      Description = expense.Description,
      Labels = labels,
      Participants = participants,
      CreationTime = expense.CreationTime,
      ExpenseTime = expense.ExpenseTime,
      LastUpdateTime = expense.LastUpdateTime,
      Payers = payers
    };

    return Results.Ok(response);
  }
}

public class GetExpenseResponse
{
  public string Id { get; set; }
  public string GroupId { get; set; }
  public string Description { get; set; }
  public string Amount { get; set; }
  public string Currency { get; set; }
  public ICollection<LabelResponse> Labels { get; set; }
  public ICollection<ParticipantResponse> Participants { get; set; }
  public ICollection<PayerResponse> Payers { get; set; }
  public DateTime ExpenseTime { get; set; }
  public DateTime CreationTime { get; set; }
  public DateTime LastUpdateTime { get; set; }
  // public UserResponse Creator { get; set; }
}