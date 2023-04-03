using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Groups.Requests;

public class GroupResponse : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string OwnerId { get; set; }

  public string Title { get; set; }

  public ICollection<Label> Labels { get; set; } = new List<Label>();

  public string BaseCurrency { get; set; }

  public ICollection<GroupMemberWithNameAndType> Members { get; set; } = new List<GroupMemberWithNameAndType>();


}
