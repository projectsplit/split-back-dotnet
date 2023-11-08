namespace SplitBackApi.Api.Models;
public class AuthedUserPendingTransaction {

  public bool UserIsSender { get; set; }

  public bool UserIsReceiver { get; set; }

  public decimal Amount { get; set; }

  public string Currency { get; set; }
}