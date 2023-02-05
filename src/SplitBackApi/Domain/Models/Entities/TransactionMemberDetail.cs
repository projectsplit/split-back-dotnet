using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class TransactionMemberDetail {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = null!;

  public decimal Lent { get; set; }

  public decimal Borrowed { get; set; }

  public decimal UserPaid { get; set; }

  public decimal UserShare { get; set; }

  public bool IsTransfer { get; set; }

  public string IsoCode { get; set; } = null!;
}