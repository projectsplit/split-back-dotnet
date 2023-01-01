using MongoDB.Bson;

namespace SplitBackApi.Entities;

public class Session : Entity<ObjectId> {

  public string RefreshToken { get; set; } = String.Empty;

  public ObjectId UserId { get; set; } = default!;

  public string Unique { get; set; } = String.Empty;
}
