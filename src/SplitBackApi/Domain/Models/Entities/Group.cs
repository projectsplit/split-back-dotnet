using MongoDB.Bson;


namespace SplitBackApi.Domain;

public class Group : EntityBase {

  public string Title { get; set; } = String.Empty;

  public ObjectId CreatorId { get; set; }

  public ICollection<ObjectId> Members { get; set; } = new List<ObjectId>();

  public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

  public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();

  public ICollection<Label> Labels { get; set; } = new List<Label>();

  public string BaseCurrencyCode { get; set; } = string.Empty;

  public ICollection<Expense> DeletedExpenses {get;set;} = new List<Expense>();
  
  public ICollection<Transfer> DeletedTransfers {get;set;} = new List<Transfer>();
}
