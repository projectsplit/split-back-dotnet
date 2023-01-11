using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Group : EntityBase {

  public string Title { get; set; } = String.Empty;
  
  [BsonRepresentation(BsonType.ObjectId)] 
  public string CreatorId { get; set; } = string.Empty;
  
  [BsonRepresentation(BsonType.ObjectId)]
  public ICollection<string> Members { get; set; } = new List<string>();
  
  public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
  
  public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
  
  public ICollection<Label> Labels { get; set; } = new List<Label>();
  
  public string BaseCurrencyCode { get; set; } = string.Empty;
}
