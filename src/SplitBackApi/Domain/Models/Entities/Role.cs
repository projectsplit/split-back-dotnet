using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;
// [BsonKnownTypes(typeof(AdminRole), typeof(EveryoneRole))]
public class Role {
  public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
  public string Title { get; set; } = null!;
  public Permissions Permissions { get; set; }
}