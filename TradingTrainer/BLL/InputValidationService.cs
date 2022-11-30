using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;

namespace TradingTrainer.BLL
{
    public class InputValidationService : IInputValidationService
    {
        private readonly Regex _symbolPattern = new Regex("^[A-Z0-9\\.]{1,20}$");
        private readonly Regex _emailPattern = new Regex("^[a-zA-Z\\#\\!\\%\\$\\‘\\&\\+\\*\\–\\/\\=\\?\\^_\\`\\.\\{\\|\\}\\~]+@[a-zA-Z0-9\\-\\.]{1,63}$");
        public InputValidationService() { }

        /**
         * This method validates the stock count input value (used for buying and selling stock).
         * Throws an ArgumentException exception if the value is not valid
         */
        public bool ValidateStockCount(int count) {

            /**
             * This method validates the stock count given as input to the trading controller.
             */
            if (count < 1)
            {
                // input value is not valid
                throw new ArgumentException("The provided count value is not valid. It must be an integer greater than 0.");
            }
            return true;
        }

        /**
         * This method validates the user id given as input to the trading controller.
         */
        public bool ValidateUserId(int userId) {
            if (userId < 1)
            {
                // User id is not valid
                throw new ArgumentException("The provided userId is not valid. It must be an integer greater than 0.");
            }
            return true;
        }

        /**
         * This method validates the stock symbol given as input to the TradingController endpoints
         */
        public bool ValidateStockSymbol(string symbol) {
            if (!_symbolPattern.IsMatch(symbol))
            {
                // Search keyword not valid
                throw new ArgumentException("The provided search symbol is not valid.");
            }
            return true;
        }

        /**
         * This method validates the search keywords given as input to the search endpoints
         */
        public bool ValidateSearchKeyword(string keyword) {
            if (keyword is null || keyword.Length > 50)
            {
                // Search keyword not valid
                throw new ArgumentException("The provided search keyword is not valid (longer than 50 characters)");
            }
            return true;
        }

        public bool ValidateUsername(string username) {
            if (!_emailPattern.IsMatch(username)) {
                // Search keyword not valid
                throw new ArgumentException("The provided username is not valid");
            }
            return true;
        }

        /**
         * This method tries to make a call to the alpha vantage api using the provided key.
         * Throws an ArgumentException if it does not work.
         */
        public async Task<bool> ValidateAlphaKey(string key) {
            AlphaVantageConnection alphaObj = await AlphaVantageConnection.BuildAlphaVantageConnectionAsync(key, true, 122);
            try
            {
                SearchResult test = await alphaObj.FindStockAsync("Microsoft");
            }
            catch (AlphaVantageApiCallNotPossible e) {
                // The key is not valid
                throw new ArgumentException("The provided api key is not working.");
            }

            return true;
        }


    }
}
