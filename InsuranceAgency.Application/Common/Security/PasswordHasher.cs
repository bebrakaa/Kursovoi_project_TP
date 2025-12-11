using System;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceAgency.Application.Common.Security;

/// <summary>
/// Простой хешер паролей для учебного проекта.
/// Не использовать в продакшене.
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string password, string hash) =>
        Hash(password) == hash;
}


