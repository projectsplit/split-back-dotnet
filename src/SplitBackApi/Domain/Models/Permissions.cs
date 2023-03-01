namespace SplitBackApi.Domain.Models;

[Flags]
public enum Permissions {

  WriteAccess = 1,
  CreateInvitation = 2,
  Comment = 4,
  ManageGroup = 8,
}
