using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class Budget : EntityBase
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Amount { get; set; }
    public string Currency { get; set; }
    public BudgetType BudgetType { get; set; }
    public string Day { get; set; }

}