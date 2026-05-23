namespace Banco.Dominio.Constantes;

public static class Permissions
{
    public const string RbacRead = "rbac.read";
    public const string RbacManage = "rbac.manage";
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string AccountsRead = "accounts.read";
    public const string AccountsCreate = "accounts.create";
    public const string AccountsApproveRequests = "accounts.approve_requests";
    public const string TransactionsRead = "transactions.read";
    public const string TransactionsManage = "transactions.manage";
    public const string RequestsCreate = "requests.create";
    public const string RequestsRead = "requests.read";
    public const string RequestsReview = "requests.review";
}
