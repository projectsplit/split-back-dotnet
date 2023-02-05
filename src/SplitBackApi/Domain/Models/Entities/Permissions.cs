namespace SplitBackApi.Domain;

[Flags]
public enum Permissions {

  CanInviteMembers = 1,
  CanArchiveGroup = 2,
  CanDeleteGroup = 4,
  CanEditLabels = 8,
  CanManageRoles = 16,
  CanRemoveMembers = 32,
  CanAddGuests = 64,
  CanRemoveGuests = 128,
  CanEditGroupDetails = 256,

  CanAddExpense = 512,
  CanEditExpense = 1024,
  CanDeleteExpense = 2048,
  CanCommentExpense = 4096,
  CanAddSpenderOtherThanHimself = 8192,

  CanAddTransfer = 16384,
  CanDeleteTransfer = 32768,
  CanAddTransferOnBehalfOfOtheruser = 65536
}
