using SplitBackApi.Domain;

namespace SplitBackApi.Attributes;

public class PermissionAttribute : Attribute {

  public Permissions Permissions { get; set; }

  public PermissionAttribute(Permissions permissions) {
    Permissions = permissions;
  }
}