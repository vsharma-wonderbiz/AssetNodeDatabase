namespace AssetNode.Interface
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
    }
}
