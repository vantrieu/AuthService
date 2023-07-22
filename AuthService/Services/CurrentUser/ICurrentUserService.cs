namespace AuthService.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }

        string Name { get; }

        string Email { get; }
    }
}
