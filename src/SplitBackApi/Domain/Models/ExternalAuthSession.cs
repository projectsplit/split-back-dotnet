using MongoDB.Bson.Serialization.Attributes;
namespace SplitBackApi.Domain.Models;

[BsonDiscriminator("ExternalAuth")]

public class ExternalAuthSession : Session {

}