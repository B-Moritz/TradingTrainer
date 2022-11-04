
// Webapplikasjoner oblig 1     OsloMet     30.10.2022

// This file contains code defining the main controller for the webapplication.
// It contains all the REST endpoints used to trade stocks (buying, selling and searching for stocks) 

using System.Diagnostics;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;
using EcbCurrencyInterface;
using Microsoft.AspNetCore.Mvc;
using Webapplikasjoner_oblig.DAL;
using Webapplikasjoner_oblig.Model;

namespace Webapplikasjoner_oblig.Controllers
{

    [Route("[controller]/[action]")]
    public class TradingController : ControllerBase
    {
        // The number of hours that a stock quote can be cached
        private readonly int _quoteCacheTime = 24;
        // Reference to TradingRepository used to access the database
        private readonly ITradingRepository _tradingRepo;
        // Object containing the configuration data defined in appsettings.json
        private readonly IConfiguration _config;
        // The searchResult repository used to access the database
        private readonly ISearchResultRepositry _searchResultRepositry;
        // The api key used for the AlphaVantageInterface
        private readonly string _apiKey;
        // The allowed number Alpha Vantage API calls per day
        private readonly int _alphaVantageDailyCallLimit = 122;


        public TradingController(ITradingRepository db, ISearchResultRepositry searchResultRepositry, IConfiguration config)
        {
            // Accessing databse for operations related to trading stocks
            _tradingRepo = db;
            // Accessing search results in the database
            _searchResultRepositry = searchResultRepositry;
            // Adding configuration object that contains the appsettings.json content
            _config = config;
            // The AlphaVantage API key can now be accessed:
            _apiKey = _config["AlphaVantageApi:ApiKey"];
        }

        /**
         * This method finds a search result that is stored in the database using a method from 
         * the search result repository. Null is returned if no match is found
         */
        public async Task<Model.SearchResult> FindStock(string keyword) 
        {
            Model.SearchResult? searchResult = await _searchResultRepositry.GetOneKeywordAsync(keyword.ToUpper());

            if(searchResult is null)
            {
                var SavedResult = SaveSearchResult(keyword);
                     
                if (SavedResult is null)
                {
                    return null;
                }
                return await SavedResult;
            }
            return searchResult;
        }

        /***
         * This method creates and returns a list containing all search results that are cached in the database.
         * It returns null if there are no search results in the database.
         */
        public async Task<List<Model.SearchResult>> GetAllFromDB()
        {
            // Collecting all search result objects stored in the database
            var list = _searchResultRepositry.GetAllKeywordsAsync();
            return await list;
        }

        /**
         * This method executes the search operation used to find stocks with a given keyword.
         * It is used as the endpoint for stock search on the client side. The endpoint returns a SearchResult 
         * object containing a list of StockSearchResult objects containg the IsFavorite flag, which indicates 
         * if the stock is in the watchlist of the user with the specified userId
         * Parameters: 
         *      (string) keyword: The search keyword used to match the availabel stocks.
         *      (int) userId: The userId of a user, used to identify if a stock is in the watchlist or not.
         * Return: The method returns a SearchResult object
         */
        public async Task<Model.SearchResult> GetUserSearchResult(string keyword, int userId) 
        {
            // Checking that the keyword is not an empty string
            if (keyword == "")
            {
                // Return an empty SearchResult object
                return new Model.SearchResult();
            }
            // Obtaining the Users object (entity)             
            Users curUser = await _tradingRepo.GetUsersAsync(userId);
            if (curUser is null)
            {
                throw new ArgumentException("The specified user was not recognized");
            }
            // Obtaining the watchlist of the user, adding an empty list if the favorites property is null
            List<Stocks> favoriteStockList = (curUser.Favorites is null ? new List<Stocks>() : curUser.Favorites);
            // Executing the search and making sure that the search result is stored in the database
            Model.SearchResult result = await SaveSearchResult(keyword);

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


        /**
         * A function to add a search result using a received word from client 
         * and checking if record matching exixst in database. 
         * If it exist already it checks its last update, if last update is greater than 24 
         * then removes exixting record and add new one by creating a new object of search result and passing it to  saveSearchResult in repository.
         * If there is no such record, it fetches data from api using the keyword and save it by passing it to saveSearchResult function
         */
        public async Task<Model.SearchResult> SaveSearchResult(string keyword)
        {
            // The search results are stored with keyword name as uppercase uppercase. Thus we operate with keyword in uppercase
            // to implement a non-casesensitive search feature
            keyword = keyword.ToUpper();
            // /trading/saveSearchResult?keyword=Equinor


            // Try and find a search result with given keyword in searchresults table 
            Model.SearchResult res = await _searchResultRepositry.GetOneKeywordAsync(keyword);

            // If there is no such search result stored in the database, then go a head and fetch it from 
            // Alpha vantage api
            if (res is null)
            {
                return await CreateNewSearchResult(keyword);
            }

            // If there exist a search result match then check when it was added.
            double timeSinceLastUpdate = (DateTime.Now - res.SearchTime).TotalHours;
            if (timeSinceLastUpdate >= _quoteCacheTime)
            {
                // If the search result is older than the QuoteCacheTime, delete the search resutl and create a new one.
                _searchResultRepositry.DeleteSearchResult(keyword);
                return await CreateNewSearchResult(keyword);
            }
            return res;
        }

        /**
         * This method is used to obtain a new search result from the AlphaVantage api and save it in the database.
         * The method checks that the search result does not already exist or is too young before makeing the Alpha Vantage request.
         * Parameters:
         *      (string) keyword: The keyword used for the search
         * Return: The resulting SearchResult object consisting of a list of StockSearchResult objects.
         */ 
        private async Task<Model.SearchResult> CreateNewSearchResult(string keyword)
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
            await _searchResultRepositry.SaveSearchResultAsync(modelSearchResult);

            return modelSearchResult;
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
         * This method defines the endpoint used to obtain a users position at the moment that the request is 
         * executed. A Portfolio object is returned containing data about the users investment (e.g unrealized 
         * profit/loss per stock and in total). All monetary values are in the currency specified on the user.
         * Parameters: 
         *      (int) userId: The user that the portfolio object should describe.
         * Return: The method returns a Portfolio object containing data about the portfolio of the user specified 
         * as an argument to this method.
         */
        public async Task<ActionResult> GetPortfolio(int userId)
        {
            // Obtaining the user entity from the database
            Users? curUser = await _tradingRepo.GetUsersAsync(userId);

            if (curUser == null) {
                //_logger.LogInformation("User {0} not found");
                NotFound("The provided user was not found in the database");
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
                curStockPrice = exchangeRate * (decimal) curQuote.Price;
                // Adding a formated version of the estimated price to the PortfolioStock
                newPortfolioStock.EstPrice = FormatMonetaryValue(curStockPrice, userCurrency);

                // Calculating the estimated market value of the shares owned by the user
                curStockValue = (decimal) curQuote.Price * ownership.StockCounter * exchangeRate;
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

            for (int i = 0; i < totalStockValueList.Count; i++) {
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
            // The Portfolio object is returned
            return Ok(outPortfolio);        
        }

        /**
         * This method is uesed as an endpoint for finding the watchlist/favorites of a user.
         * The favorite list contains a list of all stocks that a user has marked as a favorite.
         * Parameters:
         *      (int) userId: The user that is connected to the favorite list.
         * Return: A FavoriteList object.
         */
        public async Task<FavoriteList> GetFavoriteList(int userId)
        {
            // The favorite list is obtained from the repository
            return await _tradingRepo.GetFavoriteListAsync(userId);
        }

        /**
         * This method acts as an endpoint used to remove one specific stock from the favorite list of a specific 
         * user.
         * Parameters:
         *      (int) userId: The user connected to the favorite list
         *      (string) symbol: The stock id for identifying the stock to remove from the favorite list
         * Return: An updated FavoriteList object.
         */
        public async Task<FavoriteList> DeleteFromFavoriteList(int userId, string symbol)
        {
            // Removing the stock from the favorite list in the database
            await _tradingRepo.DeleteFromFavoriteListAsync(userId, symbol);
            // Return the current favorite list of the user
            return await GetFavoriteList(userId);
        }

        /**
         * This method is an endpoint used to add a stock to the favorite list of a user.
         * Parameters:
         *      (int) userId: The user connected to the favorite list
         *      (string) symbol: The stock id for identifying the stock to remove from the favorite list
         * Return: An updated FavoriteList object.
         */
        public async Task<FavoriteList> AddToFavoriteList(int userId, string symbol)
        {
            // Adding the stock to the favorite list in the database
            await _tradingRepo.AddToFavoriteListAsync(userId, symbol);
            // Returning the current favorite list of the user
            return await GetFavoriteList(userId);
        }

        /**
         * This method is the endpoint used to execute a buy operation for a specific user.  
         * Parameters:
         *      (int) userId: The user that is buying.
         *      (string) symbol: The identity of the stock that should be bought.
         *      (int) count: The amount of shares that should be bought of the specified stock.
         * Return: An updated Portfolio object that reflects the latest buy operation for the user.
         */
        public async Task<Portfolio> BuyStock(int userId, string symbol, int count)
        {
            // Validate count input value
            if (count < 1) {
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
            // Return the updated portfolio object
            return await GetPortfolio(userId);
        }

        /**
        * This method is the endpoint used to execute a sell operation for a specific user.  
        * Parameters:
        *      (int) userId: The user that is selling.
        *      (string) symbol: The identity of the stock that should be sold.
        *      (int) count: The amount of shares that should be sold of the specified stock
        * Return: An updated Portfolio object that reflects the latest selling operation for the user.
        */
        public async Task<Portfolio> SellStock(int userId, string symbol, int count)
        {
            // Check if the stock exists in the database
            Stocks? curStock = await _tradingRepo.GetStockAsync(symbol);
            if (curStock is null) 
            {
                throw new NullReferenceException("The stock was not found in the database");
            }
            // Validate stock counter. It must be an positive integer greather than or equal to 1
            if (count < 1) {
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
            decimal saldo = (decimal) curQuote.Price * count * exchangeRate;

            // Execute the sell transaction against the database
            await _tradingRepo.SellStockTransactionAsync(userId, symbol, saldo, count);
            // Return an updated portfolio object
            return await GetPortfolio(userId);
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
                StockQuotes newConvertedQuote = await _tradingRepo.AddStockQuoteAsync(newQuote);
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
                    StockQuotes newConvertedQuote = await _tradingRepo.AddStockQuoteAsync(newQuote);
                    curStockQuote = newConvertedQuote;
                }
            }
            return curStockQuote;
        }

        /**
         * This method is the endpoint used to obtain the Quote object of a stock
         * Parameters:
         *      (string) symbol: The stock symbol of the stock that the quote should be obtained for
         * Return: The StockQuote object matching the provided stock symbol.
         */
        public async Task<Model.StockQuote> GetStockQuote(string symbol) {
            // Get the latest stock quote
            StockQuotes curQuote = await GetUpdatedQuoteAsync(symbol);
            // Getting the stock currency
            string stockCurrency = curQuote.Stock.Currency;
            // Creating a new StockQuote object
            Model.StockQuote newStockQuote = new Model.StockQuote {
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

        /**
         * This method works as an endpoint used to obtain the trade history for a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         * Return: A list containing Trade objects
         */
        public async Task<List<Trade>> GetAllTrades(int userId)
        {
            // Get all the trades related to the provided userId
            return await _tradingRepo.GetAllTradesAsync(userId);
        }

        /**
         * This method works as an endpoint for clearing the trade history of a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         */
        public async Task ClearAllTradeHistory(int userId)
        {
            // Removing the trade records connected to the provided userId
            await _tradingRepo.ClearAllTradeHistoryAsync(userId);
        }
        
        /**
         * This method worsk as an endpoint used to obtain information about a given user
         * Parameters:
         *      (int) userId: The user to find information about
         * Return: The User object containing information about the user
         */
        public async Task<User> GetUser(int userId)
        {
            // Obtaining the user from the database
            return await _tradingRepo.GetUserAsync(userId);
        }

        /**
         * This method is used as an endpoint to update the information and settings for the given user
         * Parameter:
         *      (int) userId: The user to apply the changes to.
         * Return: An updated User object.
         */
        public async Task<User> UpdateUser(User curUser) {
            // Update the user in the database
            await _tradingRepo.UpdateUserAsync(curUser);
            // Returning the updated user object
            return await _tradingRepo.GetUserAsync(curUser.Id);
        }

        /**
         * 
         */
        public async Task CreateUser(int userId) {
            throw new NotImplementedException();
        }

        public async Task DeleteUser(int userId) {
            throw new NotImplementedException();
        }

        /**
         * This method is used as an endpoint to reset the profile of a given user. 
         * This will reset the valueSpent property to 0, the BuyingPower to 1,000,000.00 NOK and the 
         * Currency to NOK.
         * Parameters:
         *      (int) userId: The user that should be resetted.
         * Return: An updated User object.
         */
        public async Task<User> ResetProfile(int userId) {
            return await _tradingRepo.ResetPortfolio(userId);
        } 

    }
}
