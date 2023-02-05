using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Participant {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; }
  
  public decimal ContributionAmount { get; set; }
}
