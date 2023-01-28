using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Role {
  
  public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
  
  public string Title { get; set; } = string.Empty;
  
  public Permissions Permissions { get; set; }
}