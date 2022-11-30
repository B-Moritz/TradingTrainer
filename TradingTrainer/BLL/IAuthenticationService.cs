using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace TradingTrainer.BLL
{
    public interface IAuthenticationService
    {
        /**
         * This method generates a random salt text string (byte[]).
         */
        public static byte[] CreateSalt()
        {
            var saltGenerator = RandomNumberGenerator.Create();
            byte[] salt = new byte[24];
            // The byte array is filled with random bytes
            saltGenerator.GetBytes(salt);
            return salt;
        }

        /**
         * This method generates a password hash with the password and salt provided as argument to this function
         */
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

        Task<bool> LogInAsync(string username, string pwd);

        Task<bool> ResetPasswordAsync(int userId, string newPwd);

    }
}
