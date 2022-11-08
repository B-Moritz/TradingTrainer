
// Webapplikasjoner oblig 1     OsloMet     30.10.2022

// This file contains code defining the main controller for the webapplication.
// It contains all the REST endpoints used to trade stocks (buying, selling and searching for stocks) 

using System.Diagnostics;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;
using EcbCurrencyInterface;
using Microsoft.AspNetCore.Mvc;
using TradingTrainer.BLL;
using TradingTrainer.DAL;
using TradingTrainer.Model;

namespace TradingTrainer.Controllers
{

    [Route("[controller]/[action]")]
    public class TradingController : ControllerBase
    {
        private readonly ILogger<TradingController> _logger;
        private readonly ITradingService _tradingService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfiguration _config;
        private readonly string _LoginFlag = "_Login";

        public TradingController(IConfiguration config,
                                 ITradingService tradingService,
                                 ILogger<TradingController> logger,
                                 IAuthenticationService auth)
        {
            _config = config;
            _tradingService = tradingService;
            _logger = logger;
            _authenticationService = auth;
        }

        /**
         * This method defines the endpoint used to obtain a list of all stored SearchResult objects from the client.
         */
        public async Task<List<Model.SearchResult>> GetAllSearchResultsFromDB()
        {
            // Collecting all search result objects stored in the database
            return await _tradingService.GetAllSearchResults();
        }

        /**
         * This method executes the search operation used to find stocks with a given keyword.
         * It is used as the endpoint for stock search on the client side. The endpoint returns a SearchResult 
         * object containing a list of StockSearchResult objects containg the IsFavorite flag, which indicates 
         * if the stock is in the watchlist of the user with the specified userId.
         * Parameters: 
         *      (string) keyword: The search keyword used to match the availabel stocks.
         *      (int) userId: The userId of a user, used to identify if a stock is in the watchlist or not.
         * Return: The method returns a SearchResult object
         */
        public async Task<Model.SearchResult> GetUserSearchResult(string keyword, int userId) 
        {
            return await _tradingService.CreateUserSearchResult(keyword, userId);
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
            Portfolio outPortfolio;
            try
            {
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("An exception has occured while creating the current portfolio " +
                                 "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            
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
            return await _tradingService.CreateFavoriteListAsync(userId);
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
            return await _tradingService.DeleteFromFavoriteListAsync(userId, symbol);
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
            return await _tradingService.AddToFavoriteListAsync(userId, symbol);
        }

        /**
         * This method is the endpoint used to execute a buy operation for a specific user.  
         * Parameters:
         *      (int) userId: The user that is buying.
         *      (string) symbol: The identity of the stock that should be bought.
         *      (int) count: The amount of shares that should be bought of the specified stock.
         * Return: An updated Portfolio object that reflects the latest buy operation for the user.
         */
        public async Task<ActionResult> BuyStock(int userId, string symbol, int count)
        {
            await _tradingService.BuyStock(userId, symbol, count);
            return Ok(await _tradingService.CreateCurrentPortfolio(userId));
        }

        /**
        * This method is the endpoint used to execute a sell operation for a specific user.  
        * Parameters:
        *      (int) userId: The user that is selling.
        *      (string) symbol: The identity of the stock that should be sold.
        *      (int) count: The amount of shares that should be sold of the specified stock
        * Return: An updated Portfolio object that reflects the latest selling operation for the user.
        */
        public async Task<ActionResult> SellStock(int userId, string symbol, int count)
        {
            await _tradingService.SellStock(userId, symbol, count);
            return Ok(await _tradingService.CreateCurrentPortfolio(userId));
        }
        
        /**
         * This method is the endpoint used to obtain the Quote object of a stock
         * Parameters:
         *      (string) symbol: The stock symbol of the stock that the quote should be obtained for
         * Return: The StockQuote object matching the provided stock symbol.
         */
        public async Task<ActionResult> GetStockQuote(string symbol) {
            return Ok(await _tradingService.GetStockQuoteAsync(symbol));
        }

        /**
         * This method works as an endpoint used to obtain the trade history for a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         * Return: A list containing Trade objects
         */
        public async Task<ActionResult> GetAllTrades(int userId)
        {
            // Get all the trades related to the provided userId
            return Ok(await _tradingService.GetAllTradesAsync(userId));
        }

        /**
         * This method works as an endpoint for clearing the trade history of a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         */
        public async Task<ActionResult> ClearAllTradeHistory(int userId)
        {
            // Removing the trade records connected to the provided userId
            return Ok($"The trade history was cleared for user {userId}");
        }
        
        /**
         * This method worsk as an endpoint used to obtain information about a given user
         * Parameters:
         *      (int) userId: The user to find information about
         * Return: The User object containing information about the user
         */
        public async Task<ActionResult> GetUser(int userId)
        {
            // Obtaining the user from the database
            return Ok(await _tradingService.GetUserAsync(userId));
        }

        /**
         * This method is used as an endpoint to update the information and settings for the given user
         * Parameter:
         *      (int) userId: The user to apply the changes to.
         * Return: An updated User object.
         */
        public async Task<ActionResult> UpdateUser(User curUser) {
            return Ok(await _tradingService.UpdateUserAsync(curUser));
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
        public async Task<ActionResult> ResetProfile(int userId) {
            return Ok(await _tradingService.ResetProfileAsync(userId));
        }

        public async Task<ActionResult> Login(string username, string pwd) {
            try
            {
                bool isAuthenticated = await _authenticationService.LoginAsync(username, pwd);
                if (isAuthenticated)
                {
                    _logger.LogInformation($"The authentication was positive using username {username} and pwd {pwd}");
                    HttpContext.Session.SetString(_LoginFlag, "true");
                    return Ok();
                }
                _logger.LogInformation($"The authentication was negative using username {username} and pwd {pwd}");
                HttpContext.Session.SetString(_LoginFlag, "");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                // An exception was caught while trying to authenitcate the user
                _logger.LogInformation($"An exception was thrown while authenticating the user with username {username} and pwd {pwd}");
                HttpContext.Session.SetString(_LoginFlag, "");
                return Unauthorized();
            }
        }

    }
}
