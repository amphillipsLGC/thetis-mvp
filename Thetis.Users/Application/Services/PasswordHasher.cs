using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Thetis.Users.Domain;

namespace Thetis.Users.Application.Services;

public sealed class PasswordHasher : IPasswordHasher<User>
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;
    
    public string HashPassword(User user, string password)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = Rfc2898DeriveBytes.Pbkdf2(System.Text.Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA3_512, HashSize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2)
        {
            return PasswordVerificationResult.Failed;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var providedHash = Rfc2898DeriveBytes.Pbkdf2(System.Text.Encoding.UTF8.GetBytes(providedPassword), salt, Iterations, HashAlgorithmName.SHA3_512, HashSize);

        return hash.SequenceEqual(providedHash) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}