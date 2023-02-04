namespace SplitBackApi.Domain.Extensions;

public static class GroupExtensions {

  public static bool HasContentWithMember(this Group group, string memberId) {

    foreach(var expense in group.Expenses) {

      foreach(var spender in expense.Spenders) {

        if(spender.Id.ToString() == memberId) {
          return true;
        }
      }

      foreach(var participant in expense.Participants) {

        if(participant.Id.ToString() == memberId) {
          return true;
        }
      }
    }

    foreach(var transfer in group.Transfers) {
      
      if(transfer.SenderId.ToString() == memberId) {
        return true;
      }
      
      if(transfer.ReceiverId.ToString() == memberId) {
        return true;
      }
    }

    return false;
  }
}