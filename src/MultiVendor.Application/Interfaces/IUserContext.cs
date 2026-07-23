namespace MultiVendor.Application.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    string Email { get; }
    IList<string> Roles { get; }
}
