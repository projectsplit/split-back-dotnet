using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class User : EntityBase {

  public string Nickname { get; set; } = String.Empty;

  public string Email { get; set; } = String.Empty;

  public ICollection<ObjectId> Groups { get; set; } = new List<ObjectId>();
}
