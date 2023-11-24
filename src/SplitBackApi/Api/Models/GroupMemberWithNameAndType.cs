namespace SplitBackApi.Api.Models;
using SplitBackApi.Domain.Models;

public class GroupMemberWithNameAndType : Member {
  public string Name { get; set; }
  public string UserId { get; set; }
  public string MemberType { get; set; }
}