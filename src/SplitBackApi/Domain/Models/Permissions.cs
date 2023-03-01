namespace SplitBackApi.Domain;

[Flags]
public enum Permissions {

  WriteAccess = 1,
  CreateInvitation = 2,
  Comment = 4,
  ManageGroup = 8,
}
