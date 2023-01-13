using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class ExpenseSnapshot:EntityBase {
  
  public string Description { get; set; } = string.Empty;
  
  public decimal Amount { get; set; }
  
  public string CurrencyCode { get; set; } = string.Empty;
  
  public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
  
  public ICollection<Spender> Spenders { get; set; } = new List<Spender>();
  
  public ICollection<Participant> Participants { get; set; } = new List<Participant>();
  
  public ICollection<ObjectId> Labels { get; set; } = new List<ObjectId>();
  
  public ICollection<Comment> Comments { get; set; } = new List<Comment>();
  
  public bool IsDeleted { get; set; }
}
