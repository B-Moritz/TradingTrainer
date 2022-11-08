using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace TradingTrainer.BLL
{
    public interface IAuthenticationService
    {
        public static byte[] CreateSalt()
        {
            var saltGenerator = RandomNumberGenerator.Create();
            byte[] salt = new byte[24];
            saltGenerator.GetBytes(salt);
            return salt;
        }

        public static byte[] GetHash(string pwd, byte[] salt)
        {

            return KeyDerivation.Pbkdf2(
                    password: pwd,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 1000,
                    numBytesRequested: 32
            );
        }

        Task<bool> LoginAsync(string username, string pwd);

    }
}
