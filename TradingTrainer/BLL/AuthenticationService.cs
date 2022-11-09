using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
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
    }
}
