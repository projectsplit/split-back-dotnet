using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Participant {
  
  public ObjectId MemberId { get; set; }
  
  public decimal ContributionAmount { get; set; }
}
