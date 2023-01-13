using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Participant {
  
  public ObjectId Id { get; set; }
  
  public decimal ContributionAmount { get; set; }
}
