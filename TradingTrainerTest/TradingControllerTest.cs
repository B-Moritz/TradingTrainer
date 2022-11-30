
// Resource used to create the unit tests: https://softchris.github.io/pages/dotnet-moq.html#full-code

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TradingTrainer.BLL;
using TradingTrainer.Controllers;
using TradingTrainer.DAL;
using TradingTrainer.Model;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlphaVantageInterface.Models;
using StockQuote = AlphaVantageInterface.Models.StockQuote;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using AlphaVantageInterface;
using EcbCurrencyInterface;
using System.Data;

namespace TradingTrainerTest
{


    public class TradingControllerTest
    {
        private TradingService tradingService;
        private Mock<ILogger<TradingService>> logger;
        private Mock<ISearchResultRepositry> serchRepo;
        private Mock<IConfiguration> config;
        private Mock<ITradingRepository> tradingRepo;
        private Mock<IInputValidationService> inputValidationService;
        private Mock<ITradingService> tradingServiceMoq;
        private Mock<TradingController> tradingController;


        public TradingControllerTest()
        {
            logger = new Mock<ILogger<TradingService>>();
            serchRepo = new Mock<ISearchResultRepositry>();
            config = new Mock<IConfiguration>();
            tradingRepo = new Mock<ITradingRepository>();
            tradingServiceMoq = new Mock<ITradingService>();
            tradingController = new Mock<TradingController>();
            inputValidationService = new Mock<IInputValidationService>();
        }

        // This method tests the GetFavoriteListAsync method used to create the favorite list object
        // that is returned to the client.
        [Fact]
        public async Task GetFavoriteListAsync()
        {
            // arange
            DateTime timeNow = DateTime.Now;

            List<StockBase>? stockfavorits = new List<StockBase>();
            StockBase curstockDetail;
            curstockDetail = new StockBase
            {
                StockName = "facebook",
                Symbol = "fbc",
                Type = "Equity",
                LastUpdated = timeNow,
                StockCurrency = "USD"
            };
            stockfavorits.Add(curstockDetail);
            var curentFavorite = new FavoriteList
            {
                LastUpdated = timeNow,
                StockList = stockfavorits
            };

            var mockStock = new Stocks
            {
                StockName = "facebook",
                Symbol = "fbc",
                Type = "Equity",
                LastUpdated = timeNow,
                Currency = "USD"
            };
            List<Stocks> mockStocks = new List<Stocks>() { mockStock };

            tradingRepo.Setup(s => s.GetFavoriteListAsync(1)).ReturnsAsync(mockStocks);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);

            // act
            FavoriteList result = await tradingService.CreateFavoriteListAsync(1);
            result.LastUpdated = timeNow;
            string obj1 = JsonSerializer.Serialize(curentFavorite);
            string obj2 = JsonSerializer.Serialize(result);

            // assert
            Assert.Equal(obj1, obj2);

        }

        // This method test the getUserAsync method in the TradingService class. 
        // It is obtaining a user object from the database (mocked) and converts it to a client user object
        [Fact]
        public async Task GetUserAsync()
        {
            // arrange
            var userMock = new Users
            {
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                UsersId = 1,
                Password = Convert.FromBase64String("FBqAM9fp5mfCjyAjW0ukPtSv7YTIm0lwg02ulO8pKaw="),
                Salt = Convert.FromBase64String("x2FRQXYkGrIZ0vqUeY103YG2Nnswwp0h"),
                AlphaVantageApiKey = "FDSFDSSDG5",
                FundsAvailable = 1000000M,
                FundsSpent = 0,
                PortfolioCurrency = "NOK"

            };
            User expectedResultUser = new User
            {
                Id = 1,
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                AlphaVantageApiKey = "FDSFDSSDG5",
                FundsAvailable = String.Format("{0:N} {1}", 1000000M, "NOK"),
                FundsSpent = String.Format("{0:N} {1}", 0, "NOK"),
                Currency = "NOK"
            };


            tradingRepo.Setup(g => g.GetUsersAsync(1)).ReturnsAsync(userMock);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);
            //act
            User getUser = await tradingService.GetUserAsync(1);
            string obj11 = JsonSerializer.Serialize(expectedResultUser);
            string obj22 = JsonSerializer.Serialize(getUser);

            //assert
            Assert.Equal(obj11, obj22);
        }

        // This method is testing the CreateNewStockQuoteEntity method. This method converts an AlphaVantage stock quote 
        // to an StockQuote db entity.
        [Fact]
        public async Task CreateNewStockQuoteEntity()
        {
            // arrange
            DateTime timeNow = DateTime.Now;
            // Creating teh AlphaVantage StockQuote Object
            var testStock = new StockQuote
            {
                Symbol = "DNB",
                Open = 12.89M,
                Low = 12.32M,
                High = 12.895m,
                Price = 12.4m,
                Volume = 12439112,
                PreviousClose = 12.94m,
                Change = -0.54m,
                ChangePercent = "-4.1731 %",
                LatestTradingDay = "2022-11-29"
            };

            var expectedStock = new StockQuotes
            {
                StocksId = "DNB",
                Open = 12.89M,
                Low = 12.32M,
                High = 12.895m,
                Price = 12.4m,
                Volume = 12439112,
                PreviousClose = 12.94m,
                Change = -0.54m,
                ChangePercent = "-4.1731 %",
                LatestTradingDay = new DateTime(2022, 11, 29),
                Timestamp = timeNow

            };

            var tradingServises = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);

            //act
            StockQuotes actualStockQuote = tradingServises.CreateNewStockQuoteEntity(testStock);
            actualStockQuote.Timestamp = timeNow;
            string obj1 = JsonSerializer.Serialize(expectedStock);
            string obj2 = JsonSerializer.Serialize(actualStockQuote);

            //assert
            Assert.Equal(obj1, obj2);

        }

        /*This method is testing the CreateCurrentPortfolio which is creating the portfolio values*/
        [Fact]
        public async Task CreateCurrentPortfolio()
        {
            //arrange
            DateTime timeNow = DateTime.Now;
            
            var mockStock = new Stocks
            {
                StockName = "DNB AS",
                Symbol = "DNB",
                Type = "Equity",
                LastUpdated = timeNow,
                Currency = "USD"
            };
            // Create a ownership list for the user object
            List<StockOwnerships>? ownerships = new List<StockOwnerships>();
            var stockDetail = new StockOwnerships
            {
                UsersId = 1,
                StocksId = "DNB",
                StockCounter = 3,
                SpentValue = 100,
                Stock = mockStock,
            };
            ownerships.Add(stockDetail);

            var testUser = new Users
            {
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                UsersId = 1,
                Password = Convert.FromBase64String("FBqAM9fp5mfCjyAjW0ukPtSv7YTIm0lwg02ulO8pKaw="),
                Salt = Convert.FromBase64String("x2FRQXYkGrIZ0vqUeY103YG2Nnswwp0h"),
                AlphaVantageApiKey = "FDSFDSSDG5",
                FundsAvailable = 1000000M,
                FundsSpent = 0,
                PortfolioCurrency = "NOK",
                Portfolio = ownerships
            };

            //List<StockOwnerships> mockStocks = new List<StockOwnerships>() { stockDetail };
            // Geting stock from alphavantage
            AlphaVantageConnection AlphaV = await AlphaVantageConnection.BuildAlphaVantageConnectionAsync("KD4LAWSUOLJ9TW05", true, 122);
            AlphaVantageInterface.Models.StockQuote testQuote = await AlphaV.GetStockQuoteAsync("DNB");
            //Getting exchange rate
            decimal exchangeRate = await EcbCurrencyHandler.GetExchangeRateAsync(mockStock.Currency, testUser.PortfolioCurrency);

            StockQuotes dbStockQuote = new StockQuotes {
                StocksId = testQuote.Symbol,
                Stock = mockStock,
                Timestamp = timeNow,
                Open = testQuote.Open,
                High = testQuote.High,
                Low = testQuote.Low,
                Price = testQuote.Price,
                Volume = testQuote.Volume,
             
                PreviousClose = testQuote.PreviousClose,
                Change = testQuote.Change,
                ChangePercent = testQuote.ChangePercent
            };

            //await _tradingRepo.GetStockQuoteAsync(symbol)
            tradingRepo.Setup(p => p.GetStockQuoteAsync("DNB")).ReturnsAsync(dbStockQuote);
            // Mocking get user with input parameter userId set to 1
            tradingRepo.Setup(p => p.GetUsersAsync(1)).ReturnsAsync(testUser);
            // Create the tradingService object used to create the actual portfolio
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);

            // Create the expected portfolio object
            List<StockPortfolio>? portStock = new List<StockPortfolio>();
            var currentPortfolio = new StockPortfolio
            {
                StockName = "DNB AS",
                Symbol = "DNB",
                Type = "Equity",
                StockCurrency = "USD",
                Quantity = 3,
                EstPrice = String.Format("{0:N} {1}", exchangeRate * testQuote.Price, testUser.PortfolioCurrency),
                PortfolioPortion = "100,00%",
                EstTotalMarketValue = String.Format("{0:N} {1}", exchangeRate * testQuote.Price * 3, testUser.PortfolioCurrency),
                TotalCost = String.Format("{0:N} {1}", stockDetail.SpentValue, testUser.PortfolioCurrency),
                UnrealizedPL = String.Format("{0}{1:N} {2}",
                                (Math.Round((exchangeRate * testQuote.Price * 3) - stockDetail.SpentValue, 2) > 0 ? "+" : ""),
                                 (exchangeRate * testQuote.Price * 3) - stockDetail.SpentValue,
                                 testUser.PortfolioCurrency)

            };
            portStock.Add(currentPortfolio);

            var expectedPortfolio = new Portfolio
            {
                LastUpdate = timeNow,
                TotalValueSpent = String.Format("{0:N} {1}", stockDetail.SpentValue, testUser.PortfolioCurrency),
                EstPortfolioValue = String.Format("{0:N} {1}", exchangeRate * testQuote.Price * 3, testUser.PortfolioCurrency),
                PortfolioCurrency = testUser.PortfolioCurrency,
                Stocks = portStock,
                BuyingPower = String.Format("{0:N} {1}", testUser.FundsAvailable, testUser.PortfolioCurrency),
                UnrealizedPL = String.Format("{0}{1:N} {2}",
                                (Math.Round((exchangeRate * testQuote.Price * 3) - stockDetail.SpentValue, 2) > 0 ? "+" : ""),
                                 (exchangeRate * testQuote.Price * 3) - stockDetail.SpentValue,
                                 testUser.PortfolioCurrency)
            };


            // Get the actual portfolio
            Portfolio actualPortfolio = await tradingService.CreateCurrentPortfolio(1);
            actualPortfolio.LastUpdate = timeNow;

            string obj1 = JsonSerializer.Serialize(expectedPortfolio);
            string obj2 = JsonSerializer.Serialize(actualPortfolio);
            // assert
            Assert.Equal(obj1, obj2);
        }

        // This method tests the GetAllTradesAsync which is a method that obtains all trade records for a user 
        // and returns a list of the trade records.
        [Fact]
        public async Task GetAllTradesAsync()
        {
            //arrange
            DateTime timeNow = DateTime.Now;
            var mockedTransaction = new Trades
            {
                TradesId = 1,
                StockCount = 3,
                TradeTime = timeNow,
                UserIsBying = true,
                Saldo = 384.42002422285022204279370207M,
                Currency = "NOK",
                StocksId = "DNB",
                UsersId = 1,
            };
            // Adding the transaction into a list of transactions
            List<Trades> newTransaction = new List<Trades>() { mockedTransaction };
            // Adding trades to user object
            var testUser = new Users
            {
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                UsersId = 1,
                Password = Convert.FromBase64String("FBqAM9fp5mfCjyAjW0ukPtSv7YTIm0lwg02ulO8pKaw="),
                Salt = Convert.FromBase64String("x2FRQXYkGrIZ0vqUeY103YG2Nnswwp0h"),
                AlphaVantageApiKey = "FDSFDSSDG5",
                FundsAvailable = 1000000M,
                FundsSpent = 0,
                PortfolioCurrency = "NOK",
                Trades = newTransaction

            };

            List<Trade>? expectedTransaction = new List<Trade>();
            Trade currTransaction;
            currTransaction = new Trade
            {
                Id = 1,
                StockSymbol = "DNB",
                Date = timeNow,
                UserId = 1,
                TransactionType = "Buying",
                StockCount = 3,
                Saldo = "384,42 NOK",
            };
            expectedTransaction.Add(currTransaction);

            // arrange
            tradingRepo.Setup(t => t.GetUsersAsync(1)).ReturnsAsync(testUser);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);

            // ACT
            List<Trade> getAllTrades = await tradingService.GetAllTradesAsync(1);
            string obj1 = JsonSerializer.Serialize(expectedTransaction);
            string obj2 = JsonSerializer.Serialize(getAllTrades);
            //assert
            Assert.Equal(obj1, obj2);
        }

        // This method tests the GetStockQuoteAsync method which obtains a stock quote object from the database
        // or alphaVantage.
        [Fact]

        public async Task GetStockQuoteAsync()
        {
            // arrange
            DateTime timeNow = DateTime.Now;

            // The stock used in the mocked stock quote
            var testStock = new Stocks //ok
            {
                StockName = "DNB ASA",
                Symbol = "DNB",
                Type = "Equity",
                LastUpdated = timeNow,
                Currency = "USD"
            };

            // The stock quote returned by mock
            StockQuotes dbStockQuote = new StockQuotes //ok
            {
                StocksId = "DNB",
                Stock = testStock,
                Timestamp = timeNow,
                Open = 13.66m,
                High = 14.08m,
                Low = 13.62m,
                Price = 13.9m,
                Volume = 1526711,
                LatestTradingDay = timeNow,
                PreviousClose = 13.08m,
                Change = 0.82m,
                ChangePercent = "6.2691%"
            };
            // The expected stock quote
            var expectedStock = new TradingTrainer.Model.StockQuote //ok
            {
                Symbol = "DNB",
                StockName = "DNB ASA",
                LastUpdated = timeNow,
                Open = "13,66 USD",
                High = "14,08 USD",
                Low = "13,62 USD",
                Price = "13,90 USD",
                Volume = 1526711,
                LatestTradingDay = timeNow,
                PreviousClose = "13,08 USD",
                Change = 0.82m.ToString(),
                ChangePercent = "6.2691%"
            };

            // Mock the GetStockQuoteAsync method of the repository (we are not testing the connection with the alpha vantage api)
            tradingRepo.Setup(p => p.GetStockQuoteAsync("DNB")).ReturnsAsync(dbStockQuote);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);
            //act
            TradingTrainer.Model.StockQuote actualQuote = await tradingService.GetStockQuoteAsync("DNB");

            string obj1 = JsonSerializer.Serialize(expectedStock);
            string obj2 = JsonSerializer.Serialize(actualQuote);

            // assert
            Assert.Equal(obj1, obj2);
        }

        // This method tests the ResetProfileAsync method used to reset the user (funds available, portfolio, watchlist, ...)
        [Fact]

        public async Task ResetProfileAsync()
        {
            User expectedUser = new User
            {
                Id = 1,
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                FundsAvailable = String.Format("{0:N} {1}", 1000000M, "NOK"),
                FundsSpent = String.Format("{0:N} {1}", 0, "NOK"),
                Currency = "NOK"
            };
            var mockUser = new Users
            {
                FirstName = "Albert",
                LastName = "Jhone",
                Email = "test@gmail.com",
                UsersId = 1,
                Password = Convert.FromBase64String("FBqAM9fp5mfCjyAjW0ukPtSv7YTIm0lwg02ulO8pKaw="),
                Salt = Convert.FromBase64String("x2FRQXYkGrIZ0vqUeY103YG2Nnswwp0h"),
                AlphaVantageApiKey = "FDSFDSSDG5",
                FundsAvailable = 1000000M,
                FundsSpent = 0,
                PortfolioCurrency = "NOK"
            };

            tradingRepo.Setup(r => r.ResetProfileAsync(1)).ReturnsAsync(mockUser);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object, inputValidationService.Object);

            User resetportfolio = await tradingService.ResetProfileAsync(1);
            string obj1 = JsonSerializer.Serialize(expectedUser);
            string obj2 = JsonSerializer.Serialize(resetportfolio);
            Assert.Equal(obj1, obj2);
        }
    }
}




