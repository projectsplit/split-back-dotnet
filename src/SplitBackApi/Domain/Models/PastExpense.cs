using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class PastExpense : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string ExpenseId { get; set; } = string.Empty;

  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;
  
  public string Amount { get; set; } = string.Empty;
  
  public string Currency { get; set; } = string.Empty;
  
  public ICollection<Payer> Payers { get; set; } = new List<Payer>();
  
  public ICollection<Participant> Participants { get; set; } = new List<Participant>();

  public DateTime ExpenseTime { get; set; }
  
  public ICollection<string> Labels { get; set; } = new List<string>();
}