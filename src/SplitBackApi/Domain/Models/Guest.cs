using MongoDB.Bson;
namespace SplitBackApi.Domain;

public class Guest : Member {

  public string Nickname { get; set; } = String.Empty;

  public string Email { get; set; } = String.Empty;

}