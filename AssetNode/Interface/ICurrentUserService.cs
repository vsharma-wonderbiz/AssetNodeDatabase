    namespace AssetNode.Interface
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
        string UserDisplayName { get; }
        bool IsAuthenticated { get; }
        string GetUserGroupIdentifier();
    }
}
