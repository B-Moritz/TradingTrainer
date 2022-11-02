// Webapplikasjoner oblig 1     OsloMet     30.10.2022

// This file contains code defining the repository used to access the Trading database and execute operations against
// the database to related to buying, selling or searching for stocks.

using Microsoft.EntityFrameworkCore;
using Webapplikasjoner_oblig.Model;
using System.Linq;
using System.Reflection;
using System.Text;
using AlphaVantageInterface.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EcbCurrencyInterface;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Webapplikasjoner_oblig.DAL
{
    public class TradingRepository : ITradingRepository
    {
        // The DbContext reference
        private readonly TradingContext _db;

        public TradingRepository(TradingContext db)
        {
            _db = db;
        }

        /**
         * This method removes the stock quotes for a given stock. 
         * Parameters:
         *      (string) symbol: The stock that the quote should be removed for
         */
        public void RemoveStockQuotes(string symbol)
        {
            // This method removes all quotes of the specified stock. There should only be one quote stored for each stock
            var removeQuotes = _db.StockQuotes.Where<StockQuotes>(t => t.StocksId == symbol);
            // Remove all quotes that are connected to the stock symbol
            _db.StockQuotes.RemoveRange(removeQuotes);
            _db.SaveChanges();
        }

        /**
         * This method finds the Stocks entity matching the stock symbol provided as arguement to the method
         * Parameters:
         *      (string) symbol: The stock symbol of the wanted stock
         * Return: A Stocks object matching the stock symbol
         */
        public async Task<Stocks?> GetStockAsync(string symbol)
        {
            // Getting the Stocks object from the database
            return await _db.Stocks.FindAsync(symbol);
        }

        /**
         * This method retreives the StockQuotes object matching the stock symbol given as input to the method.
         * Parameters:
         *      (string) symbol: The stock symbol used to identify the requested StockQuotes object.
         * Return: The StocksQuote object matching the given symbol
         */
        public async Task<StockQuotes?> GetStockQuoteAsync(string symbol)
        {
            // Get all the Stock quotes matching the stock symbol
            List<StockQuotes> foundQuotes = await _db.StockQuotes.Where(s => s.StocksId == symbol).ToListAsync();
            // Return the first Stock Quote or null if the list is empty
            return foundQuotes.FirstOrDefault();
        }

        /**
         * This method converts an AlphaVantageInterface.Model.StockQuote object to
         * DAL.StockQuotes. Then the new quote is added to the database.
         * Parameters:
         *      (AlphaVantageInterface.Models.StockQuote) stockQuote: The stock quote that should be added to the database
         * Return: The StockQuotes object that was added to the database
         */
        public async Task<StockQuotes> AddStockQuoteAsync(AlphaVantageInterface.Models.StockQuote stockQuote)
        {
            // Parse the LatestTradingDay to datetime object
            Regex LatestTradingdayPattern = new Regex("([0-9]*)-([0-9]*)-([0-9]*)");
            Match matches = LatestTradingdayPattern.Match(stockQuote.LatestTradingDay);
            GroupCollection gc = matches.Groups;
            // Converting the Alpha Vantage StockQuote to a StockQuotes object
            StockQuotes newTableRow = new StockQuotes
            {
                StocksId = stockQuote.Symbol,
                Timestamp = DateTime.Now,
                LatestTradingDay = new DateTime(
                                                int.Parse(gc[1].ToString()),
                                                int.Parse(gc[2].ToString()),
                                                int.Parse(gc[3].ToString())
                                                ),
                Open = stockQuote.Open,
                Low = stockQuote.Low,
                High = stockQuote.High,
                Price = stockQuote.Price,
                Volume = stockQuote.Volume,
                PreviousClose = stockQuote.PreviousClose,
                Change = stockQuote.Change,
                ChangePercent = stockQuote.ChangePercent,
            };

            // Adding the new object to the database.
            // InvalidOperationException will be thrown if the coresponding Stocks object is not in the database
            Stocks curStock = await _db.Stocks.SingleAsync<Stocks>(s => s.Symbol == stockQuote.Symbol);
            curStock.StockQuotes.Add(newTableRow);
            // Apply the changes to the database
            _db.SaveChanges();
            // Return the new StockQuotes object
            return newTableRow;
        }

        /**
         * This method retreives the Users entity matching the userId provided as argument.
         * Parameters:
         *      (int) userId: The userId of wanted Users entity
         * Return: The Users object with the matching userId. Null is returned if no User was found in the database
         */
        public async Task<Users?> GetUsersAsync(int userId)
        {
            // Get the user entity from database
            return await _db.Users.FindAsync(userId); ;
        }

        /**
         * This metode will obtain favorites for the user that matches the userId.
         * Parameters: 
         *      (int) userId: The user to find the favorite list for.
         *  Return: FavoriteList object containing the favorite list of the given user
         */
        public async Task<FavoriteList> GetFavoriteListAsync(int userId)
        {
            // Find the user in the database
            Users oneUser = await _db.Users.SingleAsync(u => u.UsersId == userId);
            // Getting the favorites of a user
            List<Stocks>? favorites = oneUser.Favorites;
            // Declaring the new favorite list containing StockBase objects
            List<StockBase>? stockFavorite = new List<StockBase>();
            StockBase currentStockDetail;

            foreach (Stocks currentStock in favorites)
            {
                // Foreach favorite in the favorite list, create a new StockBase object and add it to the favorites list
                currentStockDetail = new StockBase
                {
                    StockName = currentStock.StockName,
                    Symbol = currentStock.Symbol,
                    Type = currentStock.Type,
                    LastUpdated = currentStock.LastUpdated
                };
                stockFavorite.Add(currentStockDetail);
            }
            // Create the favorite list if all stocks have been iterated over.
            var currentFavorite = new FavoriteList
            {
                LastUpdated = DateTime.Now,
                StockList = stockFavorite
            };
            return currentFavorite;
        }

        /**
         * This method gets all the favorites of a user
         * Parameters:
         *      (int) userId: The user to remove the stocks from.
         */
        public async Task AddToFavoriteListAsync(int userId, string symbol)
        {
            // Getting the user and Stocks object
            Users enUser = await _db.Users.SingleAsync(u => u.UsersId == userId);
            Stocks enStock = await _db.Stocks.SingleAsync(u => u.Symbol == symbol);           
            // Adding the stock to the a favorite list
            enUser.Favorites.Add(enStock);
            // Applying the changes to db
            _db.SaveChanges();
        }
        /**
         * This method removes a stock fromt he users favorite list in the database
         * Parameters:
         *      (int) userId: The user to remove the stocks from.
         */
        public async Task DeleteFromFavoriteListAsync(int userId, string symbol)
        {
            // Users object obtained from database
            Users enUser = await _db.Users.SingleAsync(u => u.UsersId == userId);
            Stocks enStock = await _db.Stocks.SingleAsync(u => u.Symbol == symbol);
            // Performe the operatopm
            enUser.Favorites.Remove(enStock);
            // Save the changes 
            _db.SaveChanges();

        }

        /**
         * This method return the user object containing information about the user
         * Parameters:
         *      (int) userId: The user to find information about.
         * Return: The User object containing information about the user
         */
        public async Task<User> GetUserAsync(int userId)
        {
            // Get the user
            Users curUser = await _db.Users.SingleAsync(t => t.UsersId == userId);
            // Create the client User object
            User convertedUser = new User
            {
                Id = curUser.UsersId,
                FirstName = curUser.FirstName,
                LastName = curUser.LastName,
                Email = curUser.Email,
                FundsSpent = string.Format("{0:N} {1}",curUser.FundsSpent, curUser.PortfolioCurrency),
                FundsAvailable = string.Format("{0:N} {1}",curUser.FundsAvailable, curUser.PortfolioCurrency),
                Currency = curUser.PortfolioCurrency
            };
            return convertedUser;
        }

        /**
         * This method applies changes to the information or settings if the user objects 
         * Parameters:
         *      (User) curUser: The user to find information about.
         * Return: The User object containing information about the user
         */
        public async Task UpdateUserAsync(User curUser) {
            // Getting the user object
            Users oldUser = await _db.Users.SingleAsync<Users>(u => u.UsersId == curUser.Id);
            // Add the properties to the new object
            oldUser.FirstName = curUser.FirstName;
            oldUser.LastName = curUser.LastName;
            oldUser.Email = curUser.Email;

            // Change currency
            decimal exchangeRate = 1;
            if (oldUser.PortfolioCurrency != curUser.Currency)
            {
                // If the currency bewteen the stocks and user settings are different. 
                exchangeRate = await EcbCurrencyHandler.GetExchangeRateAsync(oldUser.PortfolioCurrency, curUser.Currency);
            }
            // Calculate the total funds available/spent and the portfolio currency
            oldUser.FundsAvailable = oldUser.FundsAvailable * exchangeRate;
            oldUser.FundsSpent = oldUser.FundsSpent * exchangeRate;
            oldUser.PortfolioCurrency = curUser.Currency;
            foreach (StockOwnerships ownership in oldUser.Portfolio)
            {
                // Iterating over the list of stock ownerhsips
                ownership.SpentValue = ownership.SpentValue * exchangeRate;
            }
            // applying changes to database
            _db.SaveChanges();
        }

        /**
         * This method executes all operations against the database that are needed to fullfill 
         * a sell operation in the application. The operations are applied to the database as one transaction.
         * Parameters:
         *      (int) curUser: The user that executes the sell operation
         *      (string) symbol: The stock that should be sold
         *      (decimal) saldo: The monetary value that the user receives from the transaction
         *      (int) count: The amount of shares that should be sold
         */
        public async Task SellStockTransactionAsync(int userId, string symbol, decimal saldo, int count) 
        {
            // Get the User object fromt he database - InvalidOnperationException if it does not exist
            Users curUser = await _db.Users.SingleAsync<Users>(t => t.UsersId == userId);
            // Add the saldo to the buying power of the user
            curUser.FundsAvailable += saldo;

            // Remove the stock or a shares from the portfolio
            StockOwnerships curOwnership = await _db.StockOwnerships.SingleAsync<StockOwnerships>(t => (t.StocksId == symbol) && (t.UsersId == userId));
            if (curOwnership.StockCounter < count) {
                // Check that the user has enough shares of the stock to compleate the transaction
                throw new ArgumentException("The specified amount of stocks to sell exceeds the amount of shares owned!");
            }
            // Substract from the stock counter of the ownership
            curOwnership.StockCounter -= count;
            curOwnership.SpentValue -= saldo;

            if (curOwnership.StockCounter <= 0)
            {
                // The user has no more ownership of this stock type
                curUser.Portfolio.Remove(curOwnership);
            }

            // Create a new trade object
            Trades tradeLog = new Trades {
                StockCount = count,
                TradeTime = DateTime.Now,
                UserIsBying = false,
                Saldo = saldo,
                Currency = curUser.PortfolioCurrency,
                Stock = curOwnership.Stock,
                User = curOwnership.User
            };
            // Adding the new Trade object
            _db.Trades.Add(tradeLog);
            // Applying changes to database
            await _db.SaveChangesAsync();
        }

        /**
         * This method executes all operations against the database that are needed to fullfill 
         * a buy operation in the application. The operations are applied to the database as one transaction.
         * Parameters:
         *      (Users) curUser: The user object of the user that executes the buy
         *      (Stocks) curStock: The Stocks object of the stock that should be bought
         *      (decimal) saldo: The monetary value that the user must pay to compleate the transaction
         *      (int) count: The amount of shares that should be bought
         */
        public async Task BuyStockTransactionAsync(Users curUser, Stocks curStock, decimal saldo, int count) {
            // Try to find an existing stock ownership matching the user and specified stock.
            StockOwnerships? currentOwnership = await _db.StockOwnerships.FirstOrDefaultAsync<StockOwnerships>(o => o.StocksId == curStock.Symbol && o.UsersId == curUser.UsersId);
            if (currentOwnership is null)
            {
                // The user has no existing ownership. Create a new ownership
                StockOwnerships newOwnership = new StockOwnerships
                {
                    StocksId = curStock.Symbol,
                    UsersId = curUser.UsersId,
                    StockCounter = count,
                    SpentValue = saldo,
                };
                // Adding the new ownership to database
                _db.StockOwnerships.Add(newOwnership);
            }
            else {
                // Add to the existing ownership
                currentOwnership.SpentValue += saldo;
                currentOwnership.StockCounter += count;
            }

            // Get the user object from db
            Users dbUser = _db.Users.Single(u => u.UsersId == curUser.UsersId);
            // Modify the FundsAvailable and FundsSpent properties of the user
            dbUser.FundsAvailable -= saldo;
            dbUser.FundsSpent += saldo;

            // Add a trade object to log the transaction
            Trades newBuyTradeLog = new Trades
            {
                StockCount = count,
                TradeTime = DateTime.Now,
                UserIsBying = true,
                Saldo = saldo,
                Currency = dbUser.PortfolioCurrency,
                Stock = curStock,
                User = dbUser
            };
            // Add the trade list of the user
            dbUser.Trades.Add(newBuyTradeLog);
            // Execute the transaction
            await _db.SaveChangesAsync();
        }

        /**
         * This method collects all the trade records for a spesific user. This list will consist of descriptions 
         * of buy and sell transactions executed by the user.
         * Parameters:
         *      (int) userId: The user to get the trade history for
         * Return: A list of Trade objects representing the transaction history of a user.
         */ 
        public async Task<List<Trade>> GetAllTradesAsync(int userId)
        {
            // Getting the user from db - InvalidOperationException if not found
            Users dbUser = await _db.Users.SingleAsync(u => u.UsersId == userId);
            // Getting the trades list containing the Trade records
            List<Trades> curTrades = dbUser.Trades;
            // Definition of the new transaction list containing Trade objects
            // used for representing trades on the client side
            List<Trade> transactions = new List<Trade>();

            foreach(Trades curTrade in curTrades)
            {
                // Foreach trade in the user trades list, create a new Trade object
                var newTrade = new Trade
                {
                    Id = curTrade.TradesId,
                    StockSymbol = curTrade.StocksId,
                    Date = curTrade.TradeTime,
                    UserId = curTrade.UsersId,
                    TransactionType = (curTrade.UserIsBying ? "Buying" : "Selling"),
                    StockCount = curTrade.StockCount,
                    Saldo = string.Format("{0:N} {1}", curTrade.Saldo, dbUser.PortfolioCurrency)
                };
                // Adding the new Trade object to the transaction list
                transactions.Add(newTrade);
            }
            return transactions;
        }

        /**
         * This method removes the trading history saved in the database for a given user.
         * Parameters:
         *      (int) userId: The user that the trade history should be resetted for.
         */
        public async Task ClearAllTradeHistoryAsync(int userId)
        {
            // Try to get the user object from database - throws exception if it does not exist
            Users enUser = await _db.Users.SingleAsync(u => u.UsersId == userId);
            enUser.Trades.Clear();
            // Applying changes to database
            _db.SaveChanges();

        }

        /**
         * This method executes the operations agains the database to reset the user profile.
         * First the trading history, favorites and portfolio lists are cleared. Then the buying power 
         * of the user is set to 1,000,000.00 NOK, the spent value 0 and the currency to NOK.
         * Parameters:
         *      (int) userId: The user that should be resetted.
         */
        public async Task<User> ResetPortfolio(int userId) {
            // Obtain the user entity
            Users curUser = await _db.Users.SingleAsync<Users>(u => u.UsersId == userId);
            // Remove trade history of user
            curUser.Trades.Clear();
            // Remove the favorites
            curUser.Favorites.Clear();
            // Remove the stock ownerships
            curUser.Portfolio.Clear();
            // The buying power, funds spent and currency is reset
            curUser.FundsAvailable = 1000000M;
            curUser.FundsSpent = 0;
            curUser.PortfolioCurrency = "NOK";
            // Applying the changes to the database
            _db.SaveChanges();
            // Creating the updated user object
            User outObj = new User
            {
                Id = curUser.UsersId,
                FirstName = curUser.FirstName,
                LastName = curUser.LastName,
                Email = curUser.Email,
                FundsAvailable = string.Format("{0:N} {1}", curUser.FundsAvailable, curUser.PortfolioCurrency),
                FundsSpent = string.Format("{0:N} {1}", curUser.FundsSpent, curUser.PortfolioCurrency),
                Currency = curUser.PortfolioCurrency
            };
            return outObj;
        }
        
        /**
         * This method is used to remove old cached data that is not used by the application.
         * It is used by the TradingSchemaWorker.
         */
        public async Task CleanTradingSchemaAsync() 
        {
            // This method will remove stocks, stock quotes and search results, which are older than a day

            // Get the old search results
            List<SearchResults> searchResults = await _db.SearchResults.ToListAsync();
            IEnumerable<SearchResults> oldResults = searchResults.Where(searchResult => (DateTime.Now - searchResult.SearchTimestamp).TotalHours >= 24);
            _db.SearchResults.RemoveRange(oldResults);
            // Remove all stocks that have no reference to a user, trade record or a search result
            var oldStocks = await _db.Stocks.Where(curStock => (curStock.TradeOccurances.Count() == 0) && (curStock.Owners.Count() == 0) && (curStock.SearchResults.Count() == 0) && (curStock.FavoriteUsers.Count() == 0)).ToListAsync();
            foreach (Stocks oldStock in oldStocks)
            {
                if (oldStock.StockQuotes is not null)
                {
                    // if the old stock has a stock quote, remove the quote first
                    _db.StockQuotes.RemoveRange(oldStock.StockQuotes);
                }
                // Remove the old stock
                _db.Stocks.Remove(oldStock);
            }
            // Apply the changes to database
            await _db.SaveChangesAsync();
        }
    }
}