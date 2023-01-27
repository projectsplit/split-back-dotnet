namespace SplitBackApi.Data;

public class ExpenseNotFoundException : Exception {

  public ExpenseNotFoundException(string message) : base(message) {

  }

}