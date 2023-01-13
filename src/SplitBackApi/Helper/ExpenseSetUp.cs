using NMoneys.Allocations;
using SplitBackApi.Extensions;
using SplitBackApi.Endpoints.Requests;
using NMoneys;

namespace SplitBackApi.Helper
{
public static class ExpenseSetUp
  {
    public static void AllocateAmountEqually<T>(T expenseDto) where T : IExpenseDto
    {
      if (expenseDto.SplitEqually == true)
      {
        bool success = Enum.TryParse<CurrencyIsoCode>(expenseDto.IsoCode, out CurrencyIsoCode isoCode);
        if (!success)
        {
          throw new Exception();
        }
        var money = new Money(expenseDto.Amount.ToDecimal(), isoCode);
        var DistributedAmountArr = money.Allocate(expenseDto.ExpenseParticipants.Count).ToList();
        int index = 0;
        foreach (ExpenseParticipantDto Participant in expenseDto.ExpenseParticipants)
        {
          Participant.ContributionAmount = DistributedAmountArr[index].Amount.ToString();
          index = index + 1;
        }
      }
    }
  }
}