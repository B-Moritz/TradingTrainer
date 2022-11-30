
// Webapplikasjoner oblig 1     OsloMet     30.10.2022

// This file contains code defining the main controller for the webapplication.
// It contains all the REST endpoints used to trade stocks (buying, selling and searching for stocks) 

using System.Diagnostics;
using System.Text.RegularExpressions;
using AlphaVantageInterface;
using AlphaVantageInterface.Models;
using EcbCurrencyInterface;
using Microsoft.AspNetCore.Http;
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
            _logger.LogInformation($"Endpoint GetAllSearchResultsFromDB: Handeling a request for all cached search results.");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true") {
                _logger.LogInformation($"Endpoint GetAllSearchResultsFromDB: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }

            List<Model.SearchResult> results;

            try {
                // Collecting all search result objects stored in the database
                results = await _tradingService.GetAllSearchResults();
            }
            catch (Exception generalError)
            {
                // Return internal server error if an exception was thrown
                _logger.LogError("An exception has occured while searching the stock.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation($"Endpoint GetAllSearchResultsFromDB: Returning list containing all search results to the client");
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
            _logger.LogInformation($"Endpoint GetUserSearchResult: User {userId} initiated a stock search with keyword {keyword}");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetUserSearchResult: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }

            Model.SearchResult searchResult;
            try
            {
                // Get search results from the business layer
                searchResult = await _tradingService.CreateUserSearchResult(keyword, userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetUserSearchResult: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound($"Endpoint GetUserSearchResult: User with userId={userId}, was not found");
            }
            catch (ArgumentException inputInvalid) 
            { 
                _logger.LogWarning($"Endpoint GetUserSearchResult: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (AlphaVantageApiCallNotPossible alphaVantageError)
            {
                // An error wiht the AlphaVantage Interface was identified
                _logger.LogWarning(alphaVantageError.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, alphaVantageError.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint GetUserSearchResult: An exception has occured while searching the stock.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation("Endpoint GetUserSearchResult: The Search result is returned to the client.");
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
            _logger.LogInformation($"Endpoint GetPortfolio: The portfolio of user {userId} was requested.");

            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetPortfolio: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }

            Portfolio outPortfolio;
            try
            {
                // Create the portfolio object in the business layer object from the business layer
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetPortfolio: An exception has occured while trying to find the user.\n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetPortfolio: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (AlphaVantageApiCallNotPossible alphaVantageError)
            {
                // An error wiht the AlphaVantage Interface was identified
                _logger.LogWarning(alphaVantageError.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, alphaVantageError.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("Endpoint GetPortfolio: An exception has occured while creating the current portfolio " +
                                 "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }

            // The Portfolio object is returned
            _logger.LogInformation($"Endpoint GetPortfolio: The Portfolio object for user {userId} is returned to client.");
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
            _logger.LogInformation($"Endpoint GetFavoriteList: The watchlist of user {userId} was requested.");

            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetFavoriteList: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }

            FavoriteList outFavoriteList;
            try
            {
                // Obtaining watchlist form the business layer
                outFavoriteList = await _tradingService.CreateFavoriteListAsync(userId);
            }
            catch (InvalidOperationException userNotFound)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetFavoriteList: An exception has occured while trying to find the user. \n" +
                    userNotFound.Message);
                return NotFound(userNotFound.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetFavoriteList: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("Endpoint GetFavoriteList: An exception has occured while creating the current favoriteList " +
                     generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogError("Endpoint GetFavoriteList: Returing watchlist to client.");
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
            _logger.LogInformation($"Endpoint DeleteFromFavoriteList: User {userId} is removing stock {symbol} from the watchlist.");
            
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint DeleteFromFavoriteList: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }


            FavoriteList deleteFromFavoriteList;
            try
            {
                // Obtaining the updated favorite list from the business layer
                deleteFromFavoriteList = await _tradingService.DeleteFromFavoriteListAsync(userId, symbol);
            }
            catch (InvalidOperationException userOrStockNotFound)
            {
                _logger.LogWarning("Endpoint DeleteFromFavoriteList: User or stock not found in database.\n" + userOrStockNotFound.Message);
                return NotFound(userOrStockNotFound.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint DeleteFromFavoriteList: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogWarning("Endpoint DeleteFromFavoriteList: There was en error while trying to delete from favoriteList" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogWarning($"Endpoint DeleteFromFavoriteList: The updated favorite list for user {userId} is returned to client");
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
            _logger.LogInformation($"Endpoint AddToFavoriteList: User {userId} is adding " +
                $"stock {symbol} to the watchlist.");
            
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint AddToFavoriteList: The user does not have an active session.");
                return BadRequest("User does not have an active session.");
            }
            FavoriteList addFavoriteList;
            // Adding the stock to the favorite list in the database
            try
            {
                // Obtaining updated favorite list from business layer
                 addFavoriteList = await _tradingService.AddToFavoriteListAsync(userId, symbol);
            }
            catch (InvalidOperationException userOrStockNotFound)
            {
                _logger.LogWarning("Endpoint AddToFavoriteList: User or stock not found in database.\n" + userOrStockNotFound.Message);
                return NotFound(userOrStockNotFound.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint AddToFavoriteList: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Endpoint AddToFavoriteList: There was en error while trying to add to favoriteList" + e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            _logger.LogInformation($"Endpoint AddToFavoriteList: The updated favorite list is returned to client.");
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
            _logger.LogInformation($"Endpoint BuyStock: User {userId} is buying {symbol} (count={count})");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint DeleteFromFavoriteList: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            
            Portfolio outPortfolio;
            try
            {
                // Performing the buy transaction
                await _tradingService.BuyStock(userId, symbol, count);
                // Geting an updated portfolio object
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userOrStockNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint BuyStock: An exception has occured while trying to find the user or stock. \n" +
                    userOrStockNotFoundEx.Message);
                return NotFound(userOrStockNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetAllSearchResultsFromDB: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("Endpoint BuyStock: An exception has occured while creating the current portfolio " +
                                 generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation($"Endpoint BuyStock: The buy operation was successful, returning updated portfolio");
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
        public async Task<ActionResult> SellStock(int userId, string symbol, int count)
        {
            _logger.LogInformation($"Endpoint SellStock: User {userId} is selling " +
                                   $"stock {symbol} (count={count}).");
            Portfolio outPortfolio;
            try
            {
                // Performing the sell transaction
                await _tradingService.SellStock(userId, symbol, count);
                // Obtaining the uppdated portfolio object
                outPortfolio = await _tradingService.CreateCurrentPortfolio(userId);
            }
            catch (KeyNotFoundException userOrStockNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint SellStock: An exception has occured while trying to find the user. \n" +
                    userOrStockNotFoundEx.Message);
                return NotFound(userOrStockNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint SellStock: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                // An unexpected exception was thrown, log exception and respond with InternalServerError (500)
                _logger.LogError("Endpoint SellStock: An exception has occured while creating the current portfolio " +
                                 "(TradingService.CreateCurrentPortfolio):\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation($"Endpoint SellStock: The sell operation was successful, returning updated portfolio");
            return Ok(outPortfolio);
        }

        /**
         * This method is the endpoint used to obtain the Quote object of a stock
         * Parameters:
         *      (string) symbol: The stock symbol of the stock that the quote should be obtained for
         * Return: The StockQuote object matching the provided stock symbol.
         */
        public async Task<ActionResult> GetStockQuote(string symbol) {
            _logger.LogInformation($"Endpoint GetStockQuote: A new stock quote for {symbol} was requested");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint DeleteFromFavoriteList: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            Model.StockQuote quotes;
            try
            {
                quotes = await _tradingService.GetStockQuoteAsync(symbol);
            }
            catch (KeyNotFoundException stockNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetStockQuote: An exception has occured while trying to find the stock. \n" +
                    stockNotFoundEx.Message);
                return NotFound(stockNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                // Bad input was identified
                _logger.LogWarning($"Endpoint GetStockQuote: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (AlphaVantageApiCallNotPossible alphaVantageError)
            {
                // An error wiht the AlphaVantage Interface was identified
                _logger.LogWarning("Endpoint GetStockQuote: " + alphaVantageError.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, alphaVantageError.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint GetStockQuote: An exception has occured while searching the stock.\n" + generalError.Message);                
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation($"Endpoint GetStockQuote: The stock quote is returned to client");
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
            _logger.LogInformation($"Endpoint GetAllTrades: A request for trade history by user {userId}");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetAllTrades: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            List<Trade> tradeList;
            try
            {
                tradeList = await _tradingService.GetAllTradesAsync(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetAllTrades: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetAllTrades: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint GetAllTrades: An exception has occured while getting all trades.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            // Get all the trades related to the provided userId
            _logger.LogInformation("Endpoint GetAllTrades: Returning Trade history to client");
            return Ok(tradeList);
        }

        /**
         * This method works as an endpoint for clearing the trade history of a given user.
         * Parameters:
         *      (int) userId: The user connected to the returned trade records.
         */
        public async Task<ActionResult> ClearAllTradeHistory(int userId)
        {
            _logger.LogInformation($"Endpoint ClearAllTradeHistory: A request deleting trade history by user {userId}");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint ClearAllTradeHistory: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            try {
                // Clearing trade history
                await _tradingService.ClearAllTradeHistoryAsync(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint ClearAllTradeHistory: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint ClearAllTradeHistory: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint ClearAllTradeHistory: An exception has occured with getUser.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            // Removing the trade records connected to the provided userId
            _logger.LogInformation("Endpoint ClearAllTradeHistory: The trade history was successfully deleted");
            return Ok($"The trade history was cleared for user {userId}");
        }
        
        /**
         * This method works as an endpoint used to obtain information about a given user
         * Parameters:
         *      (int) userId: The user to find information about
         * Return: The User object containing information about the user
         */
        public async Task<ActionResult> GetUser(int userId)
        {
            _logger.LogInformation($"Endpoint GetUser: A request for the user object of {userId}");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetUser: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            User user;
            try
            {
                user = await _tradingService.GetUserAsync(userId);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetUser: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetUser: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint GetUser: An exception has occured with getUser.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            // Obtaining the user from the database
            _logger.LogInformation($"Endpoint GetUser: Returning the user object of user {userId}");
            return Ok(user);
        }

        /**
         * This method works as an endpoint used to obtain the User object of from the user with an active session on the server.
         * Return: The User 
         */
        public async Task<ActionResult> GetUsername()
        {
            _logger.LogInformation($"Endpoint GetUsername: Username requested");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint GetUsername: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            // Geting the username of the active session
            string? curUsername = HttpContext.Session.GetString("username");
            User curUser;

            try
            {
                curUser = await _tradingService.GetUserAsync(curUsername);
            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint GetUsername: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint GetUsername: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint GetUsername: An exception has occured with getUser.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation("Endpoint GetUsername: The provided user object of the active user is returned");
            return Ok(curUser);
        }

        /**
         * This method is used as an endpoint to update the information and settings for the given user
         * Parameter:
         *      (int) userId: The user to apply the changes to.
         * Return: An updated User object.
         */
        public async Task<ActionResult> UpdateUser([FromBody]User curUser) {
            _logger.LogInformation($"Endpoint UpdateUser: Username requested");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint UpdateUser: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }
            User user;
            if (ModelState.IsValid)
            {
                try
                {
                    // Updating user and receiving the updated user object
                    user = await _tradingService.UpdateUserAsync(curUser);
                }
                catch (InvalidOperationException userNotFoundEx)
                {
                    // The user was not found
                    _logger.LogWarning("Endpoint UpdateUser: An exception has occured while trying to find the user. \n" +
                        userNotFoundEx.Message);
                    return NotFound(userNotFoundEx.Message);
                }
                catch (ArgumentException inputInvalid)
                {
                    _logger.LogWarning($"Endpoint UpdateUser: {inputInvalid.Message}");
                    return BadRequest(inputInvalid.Message);
                }
                catch (Exception generalError)
                {

                    _logger.LogError("Endpoint UpdateUser: An exception has occured while updating the user.\n" + generalError.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
                }
                // Obtaining the user from the database
                _logger.LogInformation($"Endpoint UpdateUser: The user {curUser.Id} was updated.");
                return Ok(user);
            }
            _logger.LogWarning("Invalid input values for the update user process.");
            return BadRequest("The provided user object is not valid!");
        }
            
        /**
         * 
         */
        public async Task<bool> CreateUser([FromBody]User user) {

            return await _tradingService.CreateUser(user);
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
            _logger.LogInformation($"Endpoint ResetProfile: Profile reset requested for {userId}");
            // Check that the user is signed in 
            if (HttpContext.Session.GetString(_loginFlag) != "true")
            {
                _logger.LogInformation($"Endpoint ResetProfile: The user does not have an active session.");
                return Unauthorized("User does not have an active session.");
            }

            User user;
            try
            {
                // Resetting profile and obtaining the updated user object
                user = await _tradingService.ResetProfileAsync(userId);
            }
            catch (InvalidOperationException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint ResetProfile: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                return NotFound(userNotFoundEx.Message);
            }
            catch (ArgumentException inputInvalid)
            {
                _logger.LogWarning($"Endpoint ResetProfile: {inputInvalid.Message}");
                return BadRequest(inputInvalid.Message);
            }
            catch (Exception generalError)
            {
                _logger.LogError("Endpoint ResetProfile: An exception has occured while reseting the profile of the user.\n" + generalError.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, generalError.Message);
            }
            _logger.LogInformation("Endpoint ResetProfile: User was successfully resetted. Returning updated profile information.");
            return Ok(user);
        }

        /**
         * This method works as an endpoint to authenticate and initiate a new session for the app user.
         * The method takes the Credentials object as innput (username and password).
         */
        public async Task<ActionResult> LogIn([FromBody]Credentials curCredentials) {
            User curUser;
            try
            {
                // Try to authenticate the user
                bool isAuthenticated = await _authenticationService.LogInAsync(curCredentials.Username, curCredentials.Password);
                if (isAuthenticated)
                {
                    // If authentication was successfull - set the login flag to true
                    _logger.LogInformation($"The authentication was positive using username {curCredentials.Username} and pwd {curCredentials.Password}");
                    HttpContext.Session.SetString(_loginFlag, "true");
                    // Add the username to the session cockie
                    HttpContext.Session.SetString("username", curCredentials.Username);
                    // Getting the user from database
                    curUser = await _tradingService.GetUserAsync(curCredentials.Username);
                }
                else 
                {
                    // Authentication failed
                    _logger.LogInformation($"Endpoint LogIn: The authentication was negative using username {curCredentials.Username} and pwd {curCredentials.Password}");
                    HttpContext.Session.SetString(_loginFlag, "");
                    HttpContext.Session.SetString("username", "");
                    return Unauthorized("Negative authentication atempt.");
                }

            }
            catch (KeyNotFoundException userNotFoundEx)
            {
                // The user was not found
                _logger.LogWarning("Endpoint LogIn: An exception has occured while trying to find the user. \n" +
                    userNotFoundEx.Message);
                // Note that Unauthorized is returned instead of not found -> does not indicate that the username was wrong
                return Unauthorized(userNotFoundEx.Message);
            }
            catch (Exception ex)
            {
                // An exception was caught while trying to authenitcate the user
                _logger.LogWarning($"Endpoint LogIn: An exception was thrown while authenticating the user with username {curCredentials.Username} and pwd {curCredentials.Password}");
                HttpContext.Session.SetString(_loginFlag, "");
                HttpContext.Session.SetString("username", "");
                return Unauthorized();
            }
            _logger.LogInformation($"Endpoint LogIn: Authentication was successfull. Returning the user object.");
            return Ok(curUser);
        }

        /**
         * This method sets the login flag to "", denying the user access to the endpoints until 
         * next authentication of user done again. 
         */
        public ActionResult LogOut() {
            HttpContext.Session.SetString(_loginFlag, "");
            return Ok("true");
        }

        /**
         * This method is used as an endpoint to change the password of a specified user. Use the 
         */
        public async Task<ActionResult> ResetPwd([FromBody]UserPwd user) {
            // Check that the user has an active session
            if (HttpContext.Session.GetString(_loginFlag) != "true") {
                return Unauthorized("The user does not have an active session on the server!");
            }
            bool isChanged = false;
            try
            {
                isChanged = await _authenticationService.ResetPasswordAsync(user.UserId, user.Password);
            }
            catch (ArgumentException e) {
                return BadRequest("The provided password is not valid.");
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
            HttpContext.Session.SetString(_loginFlag, "");
            return (Ok(isChanged));
        }

    }
}
