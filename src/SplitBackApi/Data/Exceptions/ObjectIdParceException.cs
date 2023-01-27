namespace SplitBackApi.Data;

public class ObjectIdParceException : Exception {

  public ObjectIdParceException(string message) : base("Failed to parse from string to ObjectId") {

  }

}