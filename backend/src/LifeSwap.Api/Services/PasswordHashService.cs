using LifeSwap.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace LifeSwap.Api.Services;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password)
    {
        var tempUser = new User();
        return _hasher.HashPassword(tempUser, password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var tempUser = new User();
            var result = _hasher.VerifyHashedPassword(tempUser, hash, password);
            return result == PasswordVerificationResult.Success;
        }
        catch
        {
            return false;
        }
    }
}

public interface IPasswordHashService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string hash);
}
