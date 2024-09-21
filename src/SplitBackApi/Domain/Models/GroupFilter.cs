using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class GroupFilter : EntityBase
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string GroupId { get; set; }
    public ICollection<string> PayersIds { get; set; } = new List<string>();
    public ICollection<string> ParticipantsIds { get; set; } = new List<string>();
    public ICollection<string> SendersIds { get; set; } = new List<string>();
    public ICollection<string> ReceiversIds { get; set; } = new List<string>();
    public DateTime Before { get; set; }
    public DateTime After { get; set; }
    
    //TODO: ADD RANGE AND LABELS
}