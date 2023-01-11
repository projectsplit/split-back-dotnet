using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Spender {

  public ObjectId MemberId { get; set; }

  public decimal AmountSpent { get; set; }
}
