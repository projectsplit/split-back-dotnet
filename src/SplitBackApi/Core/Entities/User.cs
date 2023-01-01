using MongoDB.Bson;

namespace SplitBackApi.Entities;

public class User : Entity<ObjectId> {

  public string Nickname { get; set; } = String.Empty;

  public string Email { get; set; } = String.Empty;

  public ICollection<ObjectId> Groups { get; set; } = new List<ObjectId>();
}
