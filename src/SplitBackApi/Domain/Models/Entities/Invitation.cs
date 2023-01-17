using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
namespace SplitBackApi.Domain;

public class Invitation : EntityBase
{
    public ObjectId Inviter { get; set; }
    [MaxLength(10)]
    public string Code { get; set; } = null!;
    public ObjectId GroupId { get; set; }

}