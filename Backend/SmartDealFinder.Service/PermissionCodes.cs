namespace SmartDealFinder.Service;

public static class PermissionCodes
{
    public const string DashboardView = "Dashboard.View";
    public const string ProductsView = "Products.View";
    public const string ProductsCreate = "Products.Create";
    public const string ProductsEdit = "Products.Edit";
    public const string ProductsDelete = "Products.Delete";
    public const string CategoriesView = "Categories.View";
    public const string CategoriesManage = "Categories.Manage";
    public const string PermissionsManage = "Permissions.Manage";
    public const string UsersManage = "Users.Manage";

    public static IReadOnlyList<string> All { get; } =
    [
        DashboardView,
        ProductsView,
        ProductsCreate,
        ProductsEdit,
        ProductsDelete,
        CategoriesView,
        CategoriesManage,
        PermissionsManage,
        UsersManage
    ];
}
