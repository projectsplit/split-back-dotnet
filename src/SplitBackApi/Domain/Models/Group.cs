using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class Group : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string OwnerId { get; set; }

  public string Title { get; set; }

  public ICollection<Label> Labels { get; set; } = new List<Label>();

  public string BaseCurrency { get; set; }
  
  public ICollection<Member> Members { get; set; } = new List<Member>();
}