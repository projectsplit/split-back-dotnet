using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Helper;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> EditExpense(IRepository repo, EditExpenseDto editExpenseDto)
    {
     var groupId = ObjectId.Parse(editExpenseDto.GroupId);
         try
         {
           var expenseValidator = new ExpenseValidator();
           var validationResult = expenseValidator.Validate(editExpenseDto);
           if (validationResult.Errors.Count > 0)
           {
             return Results.Ok(validationResult.Errors.Select(x => new
             {
               Message = x.ErrorMessage,
               Field = x.PropertyName
             }));
           }
           ExpenseSetUp.AllocateAmountEqually(editExpenseDto);
           await repo.EditExpense(editExpenseDto);

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