using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core;
using Core.Repositories;

namespace Core.Repositories
{
    public class UserRepository : BaseIntRepository<User>
    {
        public UserRepository()
        {
            Init("User", "LastLogin,PasswordHash,PasswordSalt,Name,Username,Phone,Email,Role,IsActive");
        }

        public User GetByUsername(string value)
        {
            return GetSingle("Username", value.Trim());
        }
    }
}

public class User : BaseIntModel
{
    public string Name { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string Token { get; set; }
    public int Role { get; set; }
    public RoleEnum RoleEnum => (RoleEnum)Role;
    public string RoleName => RoleEnum.ToString();
    public bool IsActive { get; set; }

    public bool ChangePassword { get; set; }


    public bool Equals(User other)
    {
        return this.Id.Equals(other.Id);
    }



    public virtual void SetPassword(string passwordText)
    {
        if (string.IsNullOrWhiteSpace(passwordText))
            return;

        PasswordSalt = GenerateSalt();
        PasswordHash = Hash(passwordText, PasswordSalt);
    }
    public virtual string Hash(string value, string salt)
    {
        var hashed = Hash(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(salt));
        return Convert.ToBase64String(hashed);
    }

    public virtual bool VerifyPassword(string password)
    {
        if (string.IsNullOrEmpty(PasswordSalt) || string.IsNullOrEmpty(password))
            return false;
        var passwordHash = Hash(password, PasswordSalt);
        return PasswordHash.SequenceEqual(passwordHash);
    }
    public virtual byte[] Hash(string value, byte[] salt)
    {
        return Hash(Encoding.UTF8.GetBytes(value), salt);
    }

    public virtual byte[] Hash(byte[] value, byte[] salt)
    {
        byte[] saltedValue = value.Concat(salt).ToArray();
        return new SHA256Managed().ComputeHash(saltedValue);
    }
    private string GenerateSalt()
    {
        //var salt = Guid.NewGuid().ToByteArray();
        //var hashed = new SHA256Managed().ComputeHash(salt);
        var random = new RNGCryptoServiceProvider();
        var salt = new byte[32]; //256 bits
        random.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

}