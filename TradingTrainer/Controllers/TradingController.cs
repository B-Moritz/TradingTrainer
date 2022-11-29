
// Webapplikasjoner oblig 1     OsloMet     30.10.2022

// This file contains code defining the main controller for the webapplication.
// It contains all the REST endpoints used to trade stocks (buying, selling and searching for stocks) 

using System.Diagnostics;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;
using EcbCurrencyInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Configuration;
using TradingTrainer.BLL;
using TradingTrainer.DAL;
using TradingTrainer.Model;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace TradingTrainer.Controllers
{

    [Route("[controller]/[action]")]
    public class TradingController : ControllerBase
    {
        private readonly ILogger<TradingController> _logger;
        private readonly ITradingService _tradingService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfiguration _config;
        private readonly string _loginFlag = "_Login";

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
        public async Task<ActionResult> GetAllSearchResultsFromDB()
        {
            // Collecting all search result objects stored in the database
            List<Model.SearchResult> results = await _tradingService.GetAllSearchResults();
                   return Ok(results);
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
        public async Task<ActionResult> GetUserSearchResult(string keyword, int userId) 
        {
            Model.SearchResult searchResult;
            try
            {
                searchResult = await _tradingService.CreateUserSearchResult(keyword, userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("An exception has occured while searching the stock.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(searchResult);
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
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
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
        public async Task<ActionResult> GetFavoriteList(int userId)
        {
            FavoriteList outFavoriteList;
            try
            {
                outFavoriteList = await _tradingService.CreateFavoriteListAsync(userId);
            }
            catch (InvalidOperationException userNotFound)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFound.Message);
                return NotFound(userNotFound.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("An exception has occured while creating the current favoriteList " +
                    "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(outFavoriteList);
        }

        /**
         * This method acts as an endpoint used to remove one specific stock from the favorite list of a specific 
         * user.
         * Parameters:
         *      (int) userId: The user connected to the favorite list
         *      (string) symbol: The stock id for identifying the stock to remove from the favorite list
         * Return: An updated FavoriteList object.
         */
        public async Task<ActionResult> DeleteFromFavoriteList(int userId, string symbol)
        {
            FavoriteList deleteFromFavoriteList;
            try
            {
                deleteFromFavoriteList = await _tradingService.DeleteFromFavoriteListAsync(userId, symbol);
            }
            catch (InvalidOperationException userOrStockNotFound)
            {
                _logger.LogWarning("User or stock not found in database.\n" + userOrStockNotFound.Message);
                return NotFound(userOrStockNotFound.Message);
            }
            catch(Exception generalError)
            {
                _logger.LogWarning("There was en error while trying to delete from favoriteList" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(deleteFromFavoriteList);
        }

        /**
         * This method is an endpoint used to add a stock to the favorite list of a user.
         * Parameters:
         *      (int) userId: The user connected to the favorite list
         *      (string) symbol: The stock id for identifying the stock to remove from the favorite list
         * Return: An updated FavoriteList object.
         */
        public async Task<ActionResult> AddToFavoriteList(int userId, string symbol)
        {
            FavoriteList addFavoriteList;
            // Adding the stock to the favorite list in the database
            try
            {
                 addFavoriteList = await _tradingService.AddToFavoriteListAsync(userId, symbol);
            }
            catch (InvalidOperationException userOrStockNotFound)
            {
                _logger.LogWarning("User or stock not found in database.\n" + userOrStockNotFound.Message);
                return NotFound(userOrStockNotFound.Message);
            }
            catch (Exception e)
            {
                _logger.LogWarning("There was en error while trying to add to favoriteList" + e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            return Ok(addFavoriteList);
            // invalid
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
            //
            //
            // Skal man implementere feilhåndtering av await _tradingService.BuyStock(userId, symbol, count); ?????????????????????????
            // ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
            //

            await _tradingService.BuyStock(userId, symbol, count);
            Portfolio outPortfolio;
            try
            {
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("An exception has occured while creating the current portfolio " +
                                 "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(outPortfolio);
        }

        /**
        * This method is the endpoint used to execute a sell operation for a specific user.  
        * Parameters:
        *      (int) userId: The user that is selling.
        *      (string) symbol: The identity of the stock that should be sold.
        *      (int) count: The amount of shares that should be sold of the specified stock
        * Return: An updated Portfolio object that reflects the latest selling operation for the user.
        */
        /*
         * 
         * 
         * public async Task<ActionResult> SellStock(int userId, string symbol, int count)
        {
            await _tradingService.SellStock(userId, symbol, count);
            return Ok(await _tradingService.CreateCurrentPortfolio(userId));
        }
        */
        public async Task<ActionResult> SellStock(int userId, string symbol, int count)
        {
            //
            //
            // Skal man implementere feilhåndtering av await _tradingService.SellStock(userId, symbol, count); ?????????????????????????
            // ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
            //

            await _tradingService.SellStock(userId, symbol, count);
            Portfolio outPortfolio;
            try
            {
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("An exception has occured while creating the current portfolio " +
                                 "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(outPortfolio);
        }

        /**
         * This method is the endpoint used to obtain the Quote object of a stock
         * Parameters:
         *      (string) symbol: The stock symbol of the stock that the quote should be obtained for
         * Return: The StockQuote object matching the provided stock symbol.
         */
        public async Task<ActionResult> GetStockQuote(string symbol) {
            Model.StockQuote quotes;
            try
            {
                quotes = await _tradingService.GetStockQuoteAsync(symbol);
            }
            //
            //
            // håndtere catch av BuildAlphaVantageConnectionAsync ?????????????????????????????????????????
            //
            //
            catch (KeyNotFoundException stockNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the stock. \n" +
                    stockNotFoundEx.Message);
                return NotFound(stockNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("An exception has occured while searching the stock.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(quotes);
        }

        /**
         * This method works as an endpoint used to obtain the trade history for a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         * Return: A list containing Trade objects
         */
        public async Task<ActionResult> GetAllTrades(int userId)
        {
            List<Trade> tradeList;
            try
            {
                tradeList = await _tradingService.GetAllTradesAsync(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("An exception has occured while getting all trades.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            // Get all the trades related to the provided userId
            return Ok(tradeList);
        }

        /**
         * This method works as an endpoint for clearing the trade history of a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         */
        public async Task<ActionResult> ClearAllTradeHistory(int userId)
        {
            // skal man ikke feilhåndtere om man ikke finner en user ????????????????????????????????????????????????????????

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
            User user;
            try
            {
                user = await _tradingService.GetUserAsync(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("An exception has occured with getUser.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            // Obtaining the user from the database
            return Ok(user);
        }

        /**
         * This method works as an endpoint used to obtain the User object of from the user with an active session on the server.
         * Return: The User 
         */
        public async Task<ActionResult> GetUsername()
        {
            try
            {
                if (HttpContext.Session.GetString(_loginFlag) != "true")
                {
                    return Unauthorized();
                }
                string curUsername = HttpContext.Session.GetString("username");
                return Ok(_tradingService.GetUserAsync(curUsername));
            }
            catch (KeyNotFoundException userNotFount)
            {
                return NotFound(userNotFount.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        //[HttpPut]
        /**
         * This method is used as an endpoint to update the information and settings for the given user
         * Parameter:
         *      (int) userId: The user to apply the changes to.
         * Return: An updated User object.
         */

        public async Task<ActionResult> UpdateUser([FromBody]User curUser) {
            User user;
            if (ModelState.IsValid)
            {
                try
                {
                    user = await _tradingService.UpdateUserAsync(curUser);
                }
                catch (InvalidOperationException userNotFoundEx)
                {
                    // The user was not found
                    _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                        userNotFoundEx.Message);
                    return NotFound(userNotFoundEx.Message);
                }
                catch (Exception generalError)
                {

                    _logger.LogError("An exception has occured while updating the user.\n" + generalError.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
                }
                // Obtaining the user from the database
                return Ok(user);
            }
            _logger.LogInformation("Updating user not completed");
            return BadRequest("Feil i inputvalidering");
        }
            
        /**
         * 
         */
        public async Task<bool> CreateUser([FromBody]User user) {

            return await _tradingService.CreateUser(user);
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
        public async Task<ActionResult> ResetProfile([FromBody]int userId) {
            User user;
            try
            {
                user = await _tradingService.ResetProfileAsync(userId);
            }
            catch (InvalidOperationException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("An exception has occured while reseting the profile of the user.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            return Ok(user);
        }

        public async Task<ActionResult> LogIn([FromBody]Credentials curCredentials) {
            try
            {
                bool isAuthenticated = await _authenticationService.LogInAsync(curCredentials.Username, curCredentials.Password);
                if (isAuthenticated)
                {
                    _logger.LogInformation($"The authentication was positive using username {curCredentials.Username} and pwd {curCredentials.Password}");
                    HttpContext.Session.SetString(_loginFlag, "true");
                    HttpContext.Session.SetString("username", curCredentials.Username);
                    return Ok(_tradingService.GetUserAsync(curCredentials.Username));
                }
                _logger.LogInformation($"The authentication was negative using username {curCredentials.Username} and pwd {curCredentials.Password}");
                HttpContext.Session.SetString(_loginFlag, "");
                HttpContext.Session.SetString("username", "");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                // An exception was caught while trying to authenitcate the user
                _logger.LogInformation($"An exception was thrown while authenticating the user with username {curCredentials.Username} and pwd {curCredentials.Password}");
                HttpContext.Session.SetString(_loginFlag, "");
                HttpContext.Session.SetString("username", "");
                return Unauthorized();
            }
        }

        public ActionResult LogOut() {
            HttpContext.Session.SetString(_loginFlag, "");
            return Ok("true");
        }
    }
}
