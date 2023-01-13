using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Spender {

  public ObjectId Id { get; set; }

  public decimal AmountSpent { get; set; }
}
