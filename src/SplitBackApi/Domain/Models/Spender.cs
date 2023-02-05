using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Spender {

  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; }

  public decimal AmountSpent { get; set; }
}
