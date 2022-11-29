using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TradingTrainer.DAL;

namespace TradingTrainer.BLL
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ITradingRepository _tradingRepo;
        public AuthenticationService(ILogger<AuthenticationService> logger, 
                                     ITradingRepository tradingRepo) {
            _logger = logger;
            _tradingRepo = tradingRepo;
        }

        public async Task<bool> LogInAsync(string username, string pwd) {
            // Get the user from the database
            Users? curUser = await _tradingRepo.GetUsersAsync(username);

            if (curUser is null) {
                // A user matching the provided username was not found in the database
                throw new KeyNotFoundException("The provided user name was not found in the database!");
            }
            // Generate hash for the provided password
            byte[] pwdHash = IAuthenticationService.GetHash(pwd, curUser.Salt);
            if (pwdHash.SequenceEqual(curUser.Password))
            {
                return true;
            }
            return false;

        }

        private bool ValidateNewPassword(string newPwd) {
            char[] specialChars = new char[] { '$', '&', '+', ',', ':', ';', '=', '?', '@', '#', '|', '\'', '<', '>', '.', '-', '^', '*', '(', ')', '%', '!' };

            // Validate the new password
            if (newPwd.Length < 9 || newPwd.Length > 30)
            {
                throw new ArgumentException("The provided password is not long enough!");
            }
            if (!newPwd.Any(char.IsDigit))
            {
                throw new ArgumentException("The provided password does not contain any numbers.");
            }
            if (!newPwd.Any(char.IsLetter))
            {
                throw new ArgumentException("The provided password does not contian any letters.");
            }
            if (!newPwd.Any(c => specialChars.Contains(c)))
            {
                throw new ArgumentException("The provided password does not contain special characters.");
            }
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPwd) {

            // Get the user from the database
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);

            if (curUser is null)
            {
                // A user matching the provided username was not found in the database
                throw new KeyNotFoundException("The provided user name was not found in the database!");
            }
            ValidateNewPassword(newPwd);

            byte[] newSalt = IAuthenticationService.CreateSalt();
            byte[] newPwdHashed = IAuthenticationService.GetHash(newPwd, newSalt);

            return await _tradingRepo.updatePwdAsync(curUser, newPwdHashed, newSalt);
        }

    }
}
