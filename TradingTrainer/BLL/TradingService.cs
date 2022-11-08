// This file contains the buisness logic used to execute trades (buy or sell), search for stocks, handle favorite stocks
// and calculate the portfolio of the user


using TradingTrainer.DAL;
using Microsoft.Extensions.Logging;
using EcbCurrencyInterface;
using TradingTrainer.Model;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace TradingTrainer.BLL
{
    public class TradingService : ITradingService
    {
        // The logger object used to log in this service class
        private readonly ILogger<TradingService> _logger;
        // The number of hours that a stock quote can be cached
        private readonly int _quoteCacheTime = 24;
        // Reference to TradingRepository used to access the database
        private readonly ITradingRepository _tradingRepo;
        // Object containing the configuration data defined in appsettings.json
        private readonly IConfiguration _config;
        // The searchResult repository used to access the database
        private readonly ISearchResultRepositry _searchResultRepo;
        // The api key used for the AlphaVantageInterface
        private readonly string _apiKey;
        // The allowed number Alpha Vantage API calls per day
        private readonly int _alphaVantageDailyCallLimit = 122;

        public TradingService(ITradingRepository tradingRepo, 
                              ILogger<TradingService> logger,
                              ISearchResultRepositry searchResultRepo, 
                              IConfiguration config) {

            _tradingRepo = tradingRepo;
            _logger = logger;
            _searchResultRepo = searchResultRepo;
            _config = config;
            _apiKey = _config["AlphaVantageApi:ApiKey"];
        }

        /**
         * This method formats the monetary value given as input to this method to a string value rounded to two decimals.
         * Furthermore, the currency code is added at the end:
         *      Example: 123 341,50 NOK
         * Parameters:
         *      (decimal) monetaryValue: The value used in the monetary representation
         *      (string) currency: The currency code that should be added to the end
         * Return: The resulting formated string presenting the monetary value is returned
         */
        private string FormatMonetaryValue(decimal monetaryValue, string currency)
        {
            return String.Format("{0:N} {1}", monetaryValue, currency);
        }

        /**
         * This method formats the monetary value given as input to this method with a "+" sign if it is 
         * positive. Furthermore, the value is rounded to two decimals and the currency code is added at the end:
         *      Example: + 123 341,50 NOK
         * Parameters:
         *      (decimal) monetaryValue: The value used in the monetary representation
         *      (string) currency: The currency code that should be added to the end
         * Return: The resulting formated string presenting the monetary value is returned
         */
        private string FormatSignedMonetaryValue(decimal monetaryValue, string currency)
        {
            return String.Format("{0}{1:N} {2}",
                                (Math.Round(monetaryValue, 2) > 0 ? "+" : ""),
                                 monetaryValue,
                                 currency);
        }

        /**
         * This method obtains the latest StockQuote containing information about the value and the volume of the stock 
         * provided as an argument to this method. The method tries to find the quote in the database. If it is not found or 
         * if it has been stored for a longer time than the number of hours defined in the _quoteCacheTime, then a new quote is 
         * retreived from the Alpha Vantage api. The new Quote is then added to the database, replacing an old quote if that 
         * quote existed.
         * Parameters:
         *      (string) symbol: The stock symbol of the stock quote that should be obtained.
         * Return: The StockQuotes object containing the 
         */
        private async Task<StockQuotes> GetUpdatedQuoteAsync(string symbol)
        {
            // Create the api object used to obtain new stock quotes
            AlphaVantageConnection AlphaV = await AlphaVantageConnection.BuildAlphaVantageConnectionAsync(_apiKey, true, _alphaVantageDailyCallLimit);
            // Trying to get the latest stock object from the the database
            StockQuotes? curStockQuote = await _tradingRepo.GetStockQuoteAsync(symbol);
            if (curStockQuote is null)
            {
                // Getting a new quote from Alpha vantage api if the stock quote was not found the in the database
                AlphaVantageInterface.Models.StockQuote newQuote = await AlphaV.GetStockQuoteAsync(symbol);
                // Adding stock quote to db and get the StockQuotes object 
                StockQuotes newQuotesEntity = CreateNewStockQuoteEntity(newQuote);
                StockQuotes newConvertedQuote = await _tradingRepo.AddStockQuoteAsync(newQuotesEntity);
                // Set the new StockQuotes object as current quote
                curStockQuote = newConvertedQuote;
            }
            else
            {
                // Check that the stored quote is not outdated
                double timeSinceLastTradeDay = (DateTime.Now - curStockQuote.LatestTradingDay.AddDays(1)).TotalHours;
                double timeSinceLastUpdate = (DateTime.Now - curStockQuote.Timestamp).TotalHours;
                if (timeSinceLastTradeDay >= _quoteCacheTime && timeSinceLastUpdate >= 1)
                {
                    // If the quote was not updated within the specified _quoteCachedTime, then a new quote is obtained from api
                    // Remove the existing stock quotes from db
                    _tradingRepo.RemoveStockQuotes(symbol);
                    AlphaVantageInterface.Models.StockQuote newQuote = await AlphaV.GetStockQuoteAsync(symbol);
                    // Adding stock quote to db
                    StockQuotes newQuotesEntity = CreateNewStockQuoteEntity(newQuote);
                    StockQuotes newConvertedQuote = await _tradingRepo.AddStockQuoteAsync(newQuotesEntity);
                    curStockQuote = newConvertedQuote;
                }
            }
            return curStockQuote;
        }

        private StockQuotes CreateNewStockQuoteEntity(AlphaVantageInterface.Models.StockQuote stockQuote)
        {
            // Parse the LatestTradingDay to datetime object
            Regex LatestTradingdayPattern = new Regex("([0-9]*)-([0-9]*)-([0-9]*)");
            Match matches = LatestTradingdayPattern.Match(stockQuote.LatestTradingDay);
            GroupCollection gc = matches.Groups;
            // Converting the Alpha Vantage StockQuote to a StockQuotes object
            StockQuotes newStockQuoteEntity = new StockQuotes
            {
                StocksId = stockQuote.Symbol,
                Timestamp = DateTime.Now,
                LatestTradingDay = new DateTime(int.Parse(gc[1].ToString()),
                                                int.Parse(gc[2].ToString()),
                                                int.Parse(gc[3].ToString())),
                Open = stockQuote.Open,
                Low = stockQuote.Low,
                High = stockQuote.High,
                Price = stockQuote.Price,
                Volume = stockQuote.Volume,
                PreviousClose = stockQuote.PreviousClose,
                Change = stockQuote.Change,
                ChangePercent = stockQuote.ChangePercent,
            };

            return newStockQuoteEntity;
        }

        /**
         * This method defines the service used to create Portfolio objects for a specific user.
         * The object is created by obtainin the users stock ownership and other attributes 
         * from the data access layer. The obtained data is used to calculate important values like the 
         * total portfolio value.
         * Parameters:
         *      (int) userId: The userId of the user that the resulting portfolio object is describing.
         * Return: Portfolio object
         */
        public async Task<Portfolio> CreateCurrentPortfolio(int userId) 
        {
            // Input validation is not implemented jet

            // Obtaining the user entity from the database
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);

            if (curUser == null)
            {
                _logger.LogInformation($"User {userId} was not found in the database");
                throw new KeyNotFoundException($"The user with UserId {userId}, was not found in the database");
            }

            // Definition and initialization of the Portfolio object that should be returned
            Portfolio outPortfolio = new Portfolio();
            // Definition and initialization of an empty StockPortfolio list used to contain the found portfolio stocks later
            outPortfolio.Stocks = new List<StockPortfolio>();

            // Definition of help variables
            StockQuotes curQuote;
            Stocks curStock;
            StockPortfolio newPortfolioStock;

            // Variable for the collective value of the portfolio (all stocks)
            decimal portfolioTotalValue = 0;
            // The total value that the user has invested at the moment
            decimal totalValueSpent = 0;

            decimal exchangeRate = 1;
            decimal curStockValue = 0;
            decimal curStockPrice = 0;
            // The currency that all monetary values should have
            string userCurrency = curUser.PortfolioCurrency;
            decimal curUnrealizedPL = 0;
            // List containing the estimated value of the shares of a stock owned by a user
            // Used to find the relative value of each stock compared to each other in the portfolio
            List<decimal> totalStockValueList = new List<decimal>();

            foreach (StockOwnerships ownership in curUser.Portfolio)
            {
                // Foreach Stocks entity that the user owns,
                // convert it to a StockPortfolio object and add to the total values

                // Initializing a new instance of the StockPortfolio object
                newPortfolioStock = new StockPortfolio();
                // Obtaining the latest quote for the current stock
                curQuote = await GetUpdatedQuoteAsync(ownership.StocksId);
                // Getting the stocks entity from the database
                curStock = ownership.Stock;

                // Adding property values to the new PortfolioStock instance

                // Add the symbol
                newPortfolioStock.Symbol = curStock.Symbol;
                // Add stock name
                newPortfolioStock.StockName = curStock.StockName;
                // Add the type (e.g equity)
                newPortfolioStock.Type = curStock.Type;
                // Add stock currency
                newPortfolioStock.StockCurrency = curStock.Currency;
                // Add the qunatity to the out object
                newPortfolioStock.Quantity = ownership.StockCounter;

                // Check the currency
                if (userCurrency != curStock.Currency)
                {
                    // If the user currency is different the stock currency, get the exchange rate from ECB
                    exchangeRate = await EcbCurrencyHandler.GetExchangeRateAsync(curStock.Currency, userCurrency);
                }
                // Calculating the estimated price obtained from the quote
                curStockPrice = exchangeRate * (decimal)curQuote.Price;
                // Adding a formated version of the estimated price to the PortfolioStock
                newPortfolioStock.EstPrice = FormatMonetaryValue(curStockPrice, userCurrency);

                // Calculating the estimated market value of the shares owned by the user
                curStockValue = (decimal)curQuote.Price * ownership.StockCounter * exchangeRate;
                // Setting the Estimated total market value of the owned shares
                newPortfolioStock.EstTotalMarketValue = FormatMonetaryValue(curStockValue, userCurrency);
                // Adding the estimated stock value to the total portfolio value
                portfolioTotalValue += curStockValue;
                // Adding the value to the list of stock values
                totalStockValueList.Add(curStockValue);

                // Setting the value for how much the user has invested in the stock
                newPortfolioStock.TotalCost = FormatMonetaryValue(ownership.SpentValue, userCurrency);
                // Adding to the total spent value for the entire portfolio
                totalValueSpent += ownership.SpentValue;

                // Finding the unrealized profit/loss of the stock
                curUnrealizedPL = curStockValue - ownership.SpentValue;
                newPortfolioStock.UnrealizedPL = FormatSignedMonetaryValue(curUnrealizedPL, userCurrency);
                // Add the new PortfolioStock object to the Portfolio object
                outPortfolio.Stocks.Add(newPortfolioStock);
            }

            for (int i = 0; i < totalStockValueList.Count; i++)
            {
                // Finding the relative value of each stock compared to the total portfolio value
                outPortfolio.Stocks[i].PortfolioPortion = String.Format("{0:N}%",
                                                                       (totalStockValueList[i] / portfolioTotalValue) * 100);
            }
            // Setting the portfolio properties
            outPortfolio.EstPortfolioValue = FormatMonetaryValue(portfolioTotalValue, userCurrency);
            outPortfolio.TotalValueSpent = FormatMonetaryValue(totalValueSpent, userCurrency);
            outPortfolio.BuyingPower = FormatMonetaryValue(curUser.FundsAvailable, userCurrency);
            // Finding the total unrealized profit/loss
            decimal unrealizedPortfolioPL = portfolioTotalValue - totalValueSpent;
            outPortfolio.UnrealizedPL = FormatSignedMonetaryValue(unrealizedPortfolioPL, userCurrency);

            outPortfolio.PortfolioCurrency = userCurrency;
            outPortfolio.LastUpdate = DateTime.Now;
            return outPortfolio;
        }

        /**
         * This method is used to obtain a new search result from the AlphaVantage api and save it in the database.
         * The method checks that the search result does not already exist or is too young before makeing the Alpha Vantage request.
         * Parameters:
         *      (string) keyword: The keyword used for the search
         * Return: The resulting SearchResult object consisting of a list of StockSearchResult objects.
         */
        private async Task<Model.SearchResult> GetNewExternalSearchResult(string keyword)
        {
            // Search result object from Model 
            var modelSearchResult = new Model.SearchResult();
            // Connection to alpha vantage api
            AlphaVantageConnection AlphaV = await AlphaVantageConnection.BuildAlphaVantageConnectionAsync(_apiKey, true, _alphaVantageDailyCallLimit);
            // Fetch stocks from api using the given name 
            var alphaObject = await AlphaV.FindStockAsync(keyword);
            // Initiate a new searchResult object
            modelSearchResult.SearchKeyword = keyword;
            modelSearchResult.SearchTime = DateTime.Now;

            // StockDetails are initilized by assigning properties from stocks. StockDetails are the added to a list
            var StockDetailsList = new List<StockSearchResult>();
            foreach (Stock stock in alphaObject.BestMatches)
            {
                StockSearchResult newStockDetail = new StockSearchResult();
                newStockDetail.StockName = stock.Name;
                newStockDetail.Symbol = stock.Symbol;
                newStockDetail.Type = stock.Type;
                newStockDetail.StockCurrency = stock.Currency;
                newStockDetail.LastUpdated = DateTime.Now;

                StockDetailsList.Add(newStockDetail);
            }

            // SearchResults list properties is assigned the list holding stockDetail objects.
            modelSearchResult.StockList = StockDetailsList;

            // SearchResult is passed to a function in searchResultRepositry to be added to the database
            await _searchResultRepo.SaveSearchResultAsync(modelSearchResult);

            return modelSearchResult;
        }

        /**
        * A function to add a search result using a received word from client 
        * and checking if record matching exixst in database. 
        * If it exist already it checks its last update, if last update is greater than 24 
        * then removes existing record and add new one by creating a new object of search result and passing it to saveSearchResult in repository.
        * If there is no such record, it fetches data from api using the keyword and save it by passing it to saveSearchResult function
        */
        public async Task<Model.SearchResult> CreateOneSearchResult(string keyword)
        {
            // The search results are stored with keyword name as uppercase uppercase. Thus we operate with keyword in uppercase
            // to implement a non-casesensitive search feature
            keyword = keyword.ToUpper();
            // /trading/saveSearchResult?keyword=Equinor


            // Try and find a search result with given keyword in searchresults table 
            Model.SearchResult? res = await _searchResultRepo.GetOneKeywordAsync(keyword);

            // If there is no such search result stored in the database, then go a head and fetch it from 
            // Alpha vantage api
            if (res is null)
            {
                return await GetNewExternalSearchResult(keyword);
            }

            // If there exist a search result match then check when it was added.
            double timeSinceLastUpdate = (DateTime.Now - res.SearchTime).TotalHours;
            if (timeSinceLastUpdate >= _quoteCacheTime)
            {
                // If the search result is older than the QuoteCacheTime, delete the search resutl and create a new one.
                _searchResultRepo.DeleteSearchResult(keyword);
                return await GetNewExternalSearchResult(keyword);
            }
            return res;
        }


        /**
         * This method defines the service used to create a SearchResult for a specific user.
         * It uses the result from the CreateOneSearchResult method and adds the IsFavorite attribute to 
         * StockSearchResult objects. This attribute is used to indicate if the stock in the search 
         * result is already in the watchlist of the user.
         * Parameters: 
         *      (string) keyword: The search keyword used to match the availabel stocks.
         *      (int) userId: The userId of a user, used to identify if a stock is in the watchlist or not.
         * Return: The method returns a SearchResult object
         */
        public async Task<Model.SearchResult> CreateUserSearchResult(string keyword, int userId)
        {
            // Checking that the keyword is not an empty string
            if (keyword == "")
            {
                // Return an empty SearchResult object
                return new Model.SearchResult();
            }
            // Obtaining the Users object (entity)             
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);
            if (curUser is null)
            {
                _logger.LogInformation($"User {userId} was not found in the database");
                throw new KeyNotFoundException("The specified user was not recognized");
            }
            // Obtaining the watchlist of the user, adding an empty list if the favorites property is null
            List<Stocks> favoriteStockList = (curUser.Favorites is null ? new List<Stocks>() : curUser.Favorites);
            // Executing the search and making sure that the search result is stored in the database
            Model.SearchResult result = await CreateOneSearchResult(keyword);

            // Going through the stocks in the search results
            foreach (StockSearchResult curStock in result.StockList)
            {
                // For each search result stock, check if it is in the watchlist
                foreach (Stocks favStock in favoriteStockList)
                {
                    if (curStock.Symbol == favStock.Symbol)
                    {
                        // Setting the IsFavorite flag on the stock if it is in the watchlist
                        curStock.IsFavorite = true;
                    }
                }

            }
            return result;
        }

        /***
         * This method creates and returns a list containing all search results that are cached in the database.
         * It returns null if there are no search results in the database.
         */
        public async Task<List<Model.SearchResult>> GetAllSearchResults()
        {
            // Collecting all search result objects stored in the database
            var list = _searchResultRepo.GetAllKeywordsAsync();
            return await list;
        }


        // -----[ Favorites ] --------------------------------------------------------------------
        public async Task<FavoriteList> CreateFavoriteListAsync(int userId)
        {
            // Input validation needs to be implemented


            // The favorite list is obtained from the repository
            List<Stocks> favorites = await _tradingRepo.GetFavoriteListAsync(userId);

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
        public async Task<FavoriteList> DeleteFromFavoriteListAsync(int userId, string symbol)
        {
            // Input validation needs to be implemented

            // Removing the stock from the favorite list in the database
            await _tradingRepo.DeleteFromFavoriteListAsync(userId, symbol);
            // Return the current favorite list of the user
            return await CreateFavoriteListAsync(userId);
        }
        public async Task<FavoriteList> AddToFavoriteListAsync(int userId, string symbol)
        {
            // Adding the stock to the favorite list in the database
            await _tradingRepo.AddToFavoriteListAsync(userId, symbol);
            // Returning the current favorite list of the user
            return await CreateFavoriteListAsync(userId);
        }

        // -----[ Buy/sell/quotes stocks ] --------------------------------------------------------------
        public async Task BuyStock(int userId, string symbol, int count)
        {
            // Validate count input value
            if (count < 1)
            {
                throw new ArgumentException("The provided count value is not valid. It must be an integer greater than 0.");
            }
            // Get the user object from database
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);
            // Verifying that a user was found
            if (curUser is null)
            {
                throw new ArgumentException("The provided userId did not match any user in the database!");
            }
            // Get the stock
            Stocks? curStock = await _tradingRepo.GetStockAsync(symbol);
            // Verify that a stock with the specified stock symbol was found
            if (curStock is null)
            {
                throw new ArgumentException("The specified stock was not found in the database");
            }

            // Calculating the saldo required to buy the specified amount of shares
            // Finding the latest stock quote containing the price per share value
            StockQuotes curQuote = await GetUpdatedQuoteAsync(symbol);
            // Finding the exchange rate
            decimal exchangeRate = 1;
            if (curUser.PortfolioCurrency != curStock.Currency)
            {
                // If the stock currency is not equal to the user currency,
                // get the exchange rate with the user currency as target
                exchangeRate = await EcbCurrencyHandler.GetExchangeRateAsync(curStock.Currency, curUser.PortfolioCurrency);
            }
            // Calculating the saldo that needs to be paid
            decimal saldo = exchangeRate * (decimal)curQuote.Price * count;

            // Checking that the user has the funds needed to perform the transaction
            if (curUser.FundsAvailable - saldo < 0)
            {
                throw new Exception("The user has not enough funds to perform this transaction!");
            }
            // Execute the buy transaction with the database
            await _tradingRepo.BuyStockTransactionAsync(curUser, curStock, saldo, count); 
        }
        public async Task SellStock(int userId, string symbol, int count)
        {
            // Check if the stock exists in the database
            Stocks? curStock = await _tradingRepo.GetStockAsync(symbol);
            if (curStock is null)
            {
                throw new NullReferenceException("The stock was not found in the database");
            }
            // Validate stock counter. It must be an positive integer greather than or equal to 1
            if (count < 1)
            {
                throw new ArgumentException("The provided count value is invalid");
            }

            // Get the updated quote for the stock
            StockQuotes curQuote = await GetUpdatedQuoteAsync(symbol);

            // Get user
            Users? identifiedUser = await _tradingRepo.GetUsersAsync(userId);
            // Verifying that a user was found
            if (identifiedUser is null)
            {
                throw new ArgumentException("The provided userId did not match any user in the database!");
            }

            // Finding the total that needs to be added to the users funds
            decimal exchangeRate = 1;
            if (identifiedUser.PortfolioCurrency != curStock.Currency)
            {
                // Get the exchange rate from Ecb if the user currency differs from the stock currency
                exchangeRate = await EcbCurrencyHandler.GetExchangeRateAsync(curStock.Currency, identifiedUser.PortfolioCurrency);
            }
            // Calculating the total saldo with the amount of stocks and correct currency
            decimal saldo = (decimal)curQuote.Price * count * exchangeRate;

            // Execute the sell transaction against the database
            await _tradingRepo.SellStockTransactionAsync(userId, symbol, saldo, count);
        }
        public async Task<Model.StockQuote> GetStockQuoteAsync(string symbol)
        {
            // Get the latest stock quote
            StockQuotes curQuote = await GetUpdatedQuoteAsync(symbol);
            // Getting the stock currency
            string stockCurrency = curQuote.Stock.Currency;
            // Creating a new StockQuote object
            Model.StockQuote newStockQuote = new Model.StockQuote
            {
                Symbol = curQuote.StocksId,
                StockName = curQuote.Stock.StockName,
                LastUpdated = curQuote.Timestamp,
                Open = FormatMonetaryValue(curQuote.Open, stockCurrency),
                High = FormatMonetaryValue(curQuote.High, stockCurrency),
                Low = FormatMonetaryValue(curQuote.Low, stockCurrency),
                Price = FormatMonetaryValue(curQuote.Price, stockCurrency),
                Volume = curQuote.Volume,
                LatestTradingDay = curQuote.LatestTradingDay,
                PreviousClose = FormatMonetaryValue(curQuote.PreviousClose, stockCurrency),
                Change = curQuote.Change.ToString(),
                ChangePercent = curQuote.ChangePercent
            };
            return newStockQuote;
        }

        // -------[ Trades ] ----------------------------------------------------------------------
        /**
         * This method collects all the trade records for a spesific user. This list will consist of descriptions 
         * of buy and sell transactions executed by the user.
         * Parameters:
         *      (int) userId: The user to get the trade history for
         * Return: A list of Trade objects representing the transaction history of a user.
         */
        public async Task<List<Trade>> GetAllTradesAsync(int userId)
        {
            // Get the user entity from server
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);
            // Getting the trades list containing the Trade records
            List<Trades> curTrades = curUser.Trades;
            // Definition of the new transaction list containing Trade objects
            // used for representing trades on the client side
            List<Trade> transactions = new List<Trade>();

            foreach (Trades curTrade in curTrades)
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
                    Saldo = string.Format("{0:N} {1}", curTrade.Saldo, curUser.PortfolioCurrency)
                };
                // Adding the new Trade object to the transaction list
                transactions.Add(newTrade);
            }
            return transactions;
        }

        public async Task ClearAllTradeHistoryAsync(int userId)
        {
            // Removing the trade records connected to the provided userId
            await _tradingRepo.ClearAllTradeHistoryAsync(userId);
        }

        // -------[ User ] ------------------------------------------------------------------------

        public async Task<User> GetUserAsync(int userId)
        {
            // Obtaining the user from the database
            Users curUser =  await _tradingRepo.GetUsersAsync(userId);
            User convertedUser = new User
            {
                Id = curUser.UsersId,
                FirstName = curUser.FirstName,
                LastName = curUser.LastName,
                Email = curUser.Email,
                FundsSpent = string.Format("{0:N} {1}", curUser.FundsSpent, curUser.PortfolioCurrency),
                FundsAvailable = string.Format("{0:N} {1}", curUser.FundsAvailable, curUser.PortfolioCurrency),
                Currency = curUser.PortfolioCurrency
            };
            return convertedUser;
        }

        public async Task<User> UpdateUserAsync(User curUser)
        {
            // Update the user in the database
            await _tradingRepo.UpdateUserAsync(curUser);
            // Returning the updated user object
            return await GetUserAsync(curUser.Id);
        }

        public async Task<User> ResetProfileAsync(int userId)
        {
            // Reset data through the data access layer
            Users curUser = await _tradingRepo.ResetProfileAsync(userId);
            // Convert the returned user entity to the User object used for presentation
            User convertedUser = new User
            {
                Id = curUser.UsersId,
                FirstName = curUser.FirstName,
                LastName = curUser.LastName,
                Email = curUser.Email,
                FundsSpent = string.Format("{0:N} {1}", curUser.FundsSpent, curUser.PortfolioCurrency),
                FundsAvailable = string.Format("{0:N} {1}", curUser.FundsAvailable, curUser.PortfolioCurrency),
                Currency = curUser.PortfolioCurrency
            };
            return convertedUser;
        }

    }
}
