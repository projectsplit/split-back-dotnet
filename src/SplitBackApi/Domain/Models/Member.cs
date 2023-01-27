using MongoDB.Bson;
namespace SplitBackApi.Domain;

public class Member
{
  public ObjectId UserId { get; set; }
  public ICollection<ObjectId> Roles { get; set; } = new List<ObjectId>();
  //public ObjectId InvitedBy { get; set; }
}