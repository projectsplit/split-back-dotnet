using MongoDB.Bson;

namespace SplitBackApi.Data.Extensions;

public static class StringExtensions {
  
  public static ObjectId ToObjectId(this string input) {
    
    return ObjectId.Parse(input);
  }
}