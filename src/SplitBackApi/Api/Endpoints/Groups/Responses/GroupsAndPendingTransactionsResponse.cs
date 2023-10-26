using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Groups.Responses;

public class GroupsAndPendingTransactionsResponse {

  public Group Group { get; set; }
  public IEnumerable<AuthedUserPendingTransaction> PendingTransactions { get; set; } = new List<AuthedUserPendingTransaction>();
}
