using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Expense : EntityBase {

  public string Description { get; set; } = string.Empty;

  public decimal Amount { get; set; }

  public ICollection<Spender> ExpenseSpenders { get; set; } = new List<Spender>();

  public ICollection<Participant> Participants { get; set; } = new List<Participant>();

  public ICollection<ObjectId> Labels { get; set; } = new List<ObjectId>();

  public string IsoCode { get; set; } = string.Empty;

  public ICollection<Comment> Comments { get; set; } = new List<Comment>();

  public ICollection<ExpenseSnapshot> History { get; set; } = new List<ExpenseSnapshot>();

  public bool IsDeleted { get; set; } = false;
}
