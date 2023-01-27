using MongoDB.Bson;
namespace SplitBackApi.Domain;

public class Role2
{
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; }
    public ICollection<Permission2> Permissions { get; set; } = new List<Permission2>();
}

public class Permission2
{
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
}

public class RolePermissionManager
{
    private readonly Dictionary<string, Role2> _roles = new Dictionary<string, Role2>();
    private readonly Dictionary<string, Permission2> _permissions = new Dictionary<string, Permission2>();

    public RolePermissionManager()
    {
        // Initialize roles and permissions
        var adminRole = new Role2 { Name = "Admin" };
        adminRole.Permissions.Add(new Permission2 { Name = "CanInviteMembers", IsEnabled = true });
        adminRole.Permissions.Add(new Permission2 { Name = "CanEditLabels", IsEnabled = true });
        _roles.Add("Admin", adminRole);

        var memberRole = new Role2 { Name = "Member" };
        memberRole.Permissions.Add(new Permission2 { Name = "CanAddExpense", IsEnabled = true });
        memberRole.Permissions.Add(new Permission2 { Name = "CanDeleteExpense", IsEnabled = false });
        _roles.Add("Member", memberRole);

        // Initialize permissions 
        _permissions.Add("CanInviteMembers", new Permission2 { Name = "CanInviteMembers", IsEnabled = true });
        _permissions.Add("CanEditLabels", new Permission2 { Name = "CanEditLabels", IsEnabled = true });
        _permissions.Add("CanAddExpense", new Permission2 { Name = "CanAddExpense", IsEnabled = true });
        _permissions.Add("CanDeleteExpense", new Permission2 { Name = "CanDeleteExpense", IsEnabled = true });
    }

    public Role2 GetRole(string roleName)
    {
        return _roles[roleName];
    }

    public Permission2 GetPermission(string permissionName)
    {
        return _permissions[permissionName];
    }

    public void EnablePermissionForRole(string roleName, string permissionName)
    {
        var role = _roles[roleName];
        var permission = _permissions[permissionName];
        permission.IsEnabled = true;
        if (!role.Permissions.Contains(permission))
        {
            role.Permissions.Add(permission);
        }
    }
}
