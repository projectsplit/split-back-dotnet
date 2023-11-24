using NMoneys;
using SplitBackApi.Api.Helper;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Services.ServiceHelpers;

public class TransactionHelper
{
  public static List<TransactionMember2> InitializeTransactionMembers(List<Member> members, string currency)
  {
    var isoCurrency = MoneyHelper.StringToIsoCode(currency);
    return members.Select(member => new TransactionMember2(member.MemberId, Money.Zero(isoCurrency), Money.Zero(currency))).ToList();
  }
  public static void CalculateTotalTakenAndGivenForEachMember(List<TransactionMember2> transactionMembers, List<Expense> expenses, List<Transfer> transfers, string currency)
  {
    var isoCurrency = MoneyHelper.StringToIsoCode(currency);

    transactionMembers.ForEach(m =>
 {
   var currencyExpenses = expenses.Where(expense => expense.Currency == currency);
   var currencyTransfersSender = transfers.Where(transfer => transfer.Currency == currency && transfer.SenderId == m.Id);
   var currencyTransfersReceiver = transfers.Where(transfer => transfer.Currency == currency && transfer.ReceiverId == m.Id);

   var TotalParticipationAmountFromExpenses = currencyExpenses.Any() ?
   currencyExpenses
   .SelectMany(expense => expense.Participants).Where(p => p.MemberId == m.Id)
   .Select(p => p.ParticipationAmount)
   .Select(amount => new Money(amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

   var TotalAmountGivenFromTransfers = currencyTransfersSender.Any() ?
   currencyTransfersSender
   .Select(t => new Money(t.Amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

   var TotalPaymentAmountFromExpenses = currencyExpenses.Any() ?
  currencyExpenses
  .SelectMany(expense => expense.Payers).Where(p => p.MemberId == m.Id)
  .Select(p => p.PaymentAmount)
  .Select(amount => new Money(amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

   var TotalAmountTakenFromTransfers = currencyTransfersReceiver.Any() ?
   currencyTransfersReceiver
   .Select(t => new Money(t.Amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

   m.TotalAmountGiven = Money.Total(TotalPaymentAmountFromExpenses.Concat(TotalAmountGivenFromTransfers));
   m.TotalAmountTaken = Money.Total(TotalParticipationAmountFromExpenses.Concat(TotalAmountTakenFromTransfers));

 });
  }
  public static (Queue<TransactionMember2>, Queue<TransactionMember2>) CalculateDebtorsAndCreditors(List<TransactionMember2> transactionMembers)
  {
    var debtors = new Queue<TransactionMember2>();
    var creditors = new Queue<TransactionMember2>();

    transactionMembers.ForEach(transactionMember =>
       {
         switch (transactionMember.TotalAmountGiven.Minus(transactionMember.TotalAmountTaken).Amount)
         {
           case < 0:
             debtors.Enqueue(transactionMember);
             break;

           case > 0:
             creditors.Enqueue(transactionMember);
             break;
         }
       });
    return (debtors, creditors);
  }

  public static void CalculatePendingTransactions(
    Queue<TransactionMember2> debtors,
    Queue<TransactionMember2> creditors,
    List<PendingTransaction2> pendingTransactions,
    string currency)
  {
    while (debtors.Count > 0 && creditors.Count > 0)
    {

      var poppedDebtor = debtors.Dequeue();
      var poppedCreditor = creditors.Dequeue();

      var debt = poppedDebtor.TotalAmountTaken.Minus(poppedDebtor.TotalAmountGiven);
      var credit = poppedCreditor.TotalAmountGiven.Minus(poppedCreditor.TotalAmountTaken);
      var diff = debt.Minus(credit).Amount;

      switch (diff)
      {
        case < 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = debt,
            Currency = currency
          });

          creditors.Enqueue(poppedCreditor with { TotalAmountTaken = poppedCreditor.TotalAmountTaken.Plus(debt) });
          break;

        case > 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = credit,
            Currency = currency
          });

          debtors.Enqueue(poppedDebtor with { TotalAmountGiven = poppedDebtor.TotalAmountGiven.Plus(credit) });
          break;

        case 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = credit, //credit == debt
            Currency = currency
          });
          break;
      }
    }
  }
}