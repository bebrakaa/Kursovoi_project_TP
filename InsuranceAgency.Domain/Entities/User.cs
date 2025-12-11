using System;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Пользователь системы.
    /// В домене храним основную информацию и роль.
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public UserRole Role { get; private set; }

        /// <summary>
        /// Хэш пароля пользователя (не сам пароль).
        /// </summary>
        public string PasswordHash { get; private set; } = null!;

        protected User() { }

        public User(string username, string email, UserRole role, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("passwordHash");

            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            Role = role;
            PasswordHash = passwordHash;
        }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");
        Email = email;
    }
    }
}
