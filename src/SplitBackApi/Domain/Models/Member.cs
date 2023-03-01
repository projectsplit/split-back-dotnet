using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

[BsonKnownTypes(typeof(UserMember), typeof(GuestMember))]
public class Member {

  public string MemberId { get; set; }
  
  public Permissions Permissions { get; set; }
}