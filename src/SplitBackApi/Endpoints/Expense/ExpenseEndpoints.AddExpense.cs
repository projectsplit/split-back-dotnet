using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> AddExpense(IRepository repo, HttpRequest request, NewExpenseDto newExpenseDto)
    {
        var groupId = new ObjectId(newExpenseDto.GroupId);
        try
        {
            var expenseValidator = new ExpenseValidator();
            var validationResult = expenseValidator.Validate(newExpenseDto);
            if (validationResult.Errors.Count > 0)
            {
                return Results.Ok(validationResult.Errors.Select(x => new
                {
                    Message = x.ErrorMessage,
                    Field = x.PropertyName
                }));
            }
            ExpenseSetUp.AllocateAmountEqually(newExpenseDto);
            await repo.AddNewExpense(newExpenseDto);

            var group = await repo.GetGroupById(groupId);
            if (group is null) throw new Exception();
            return Results.Ok(group.PendingTransactions());
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}