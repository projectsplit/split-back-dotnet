namespace SplitBackApi.Domain;
[Flags]
public enum Permissions {
  CanInviteMembers = 1,
  CanArchiveGroup = 2,
  CanDeleteGroup = 4,
  CanEditLabels = 8,
  CanManageRoles = 16,
  CanRemoveMembers = 32,
  CanEditGroupDetails = 64,

  CanAddExpense = 128,
  CanEditExpense = 256,
  CanDeleteExpense = 512,
  CanCommentExpense = 1024,
  CanAddSpenderOtherThanHimself = 2048,

  CanAddTransfer = 4096,
  CanDeleteTransfer = 8192,
  CanAddTransferOnBehalfOfOtheruser = 16384
}
