using SplitBackApi.Domain;
namespace SplitBackApi.Helper;
public class IsoCodeHelper
{
  public static IEnumerable<string> GetUniqueIsoCodes(Group group)
  {
    var expenseListsByIsoCode = group.Expenses.GroupBy(exp => exp.IsoCode);
    var transferListsByIsoCode = group.Transfers.GroupBy(tr => tr.IsoCode);

    var isoCodeList = new List<string>();
    expenseListsByIsoCode.ToList().ForEach(list => isoCodeList.Add(list.Key));
    transferListsByIsoCode.ToList().ForEach(list => isoCodeList.Add(list.Key));
    var uniqueIsoCodeList = isoCodeList.Distinct();
    return uniqueIsoCodeList;
  }
}
