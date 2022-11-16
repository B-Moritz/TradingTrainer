
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

namespace TradingTrainerTest
{
    public class TradingControllerTest
    {
        private TradingService tradingService;
        private Mock<ILogger<TradingService>> logger;
        private Mock<ISearchResultRepositry> serchRepo;
        private Mock<IConfiguration> config;
        private Mock<ITradingRepository> tradingRepo;

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

        }

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
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);

            // act
            FavoriteList result = await tradingService.CreateFavoriteListAsync(1);
            result.LastUpdated = timeNow;
            string obj1 = JsonSerializer.Serialize(curentFavorite);
            string obj2 = JsonSerializer.Serialize(result);

            // assert
            Assert.Equal(obj1, obj2);

        }

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
                FundsAvailable = String.Format("{0:N} {1}", 1000000M, "NOK"),
                FundsSpent = String.Format("{0:N} {1}", 0, "NOK"),
                Currency = "NOK"
            };


            tradingRepo.Setup(g => g.GetUsersAsync(1)).ReturnsAsync(userMock);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);
            //act
            User getUser = await tradingService.GetUserAsync(1);
            string obj11 = JsonSerializer.Serialize(expectedResultUser);
            string obj22 = JsonSerializer.Serialize(getUser);

            //assert
            Assert.Equal(obj11, obj22);

        }
        [Fact]
        public async Task CreateNewStockQuoteEntity()
        {
            // arrange
            var actualStock = new StockQuote
            {
                Open = 12.89M,
                Low = 12.32M,
                High = 12.895m,
                Price = 12.4m,
                Volume = 12439112,
                PreviousClose = 12.94m,
                Change = -0.54m,
                ChangePercent = "-4.1731 %"
            };
            var expectedStock = new StockQuotes
            {
                Open = 12.89M,
                Low = 12.32M,
                High = 12.895m,
                Price = 12.4m,
                Volume = 12439112,
                PreviousClose = 12.94m,
                Change = -0.54m,
                ChangePercent = "-4.1731 %"

            };

            //tradingRepo.Setup(s => s.CreateNewStockQuoteEntity("fbc"));
            var tradingServises = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);

            //act
            StockQuotes creatNewStockEntity = tradingServises.CreateNewStockQuoteEntity(actualStock);
            string obj1 = JsonSerializer.Serialize(expectedStock);
            string obj2 = JsonSerializer.Serialize(creatNewStockEntity);

            //assert
            Assert.Equal(obj1, obj2);

        }

        [Fact]
        public async Task CreateCurrentPortfolio()
        {
            //arrange
            DateTime timeNow = DateTime.Now;

            List<Users>? actualPortfolio = new List<Users>();

            /* List<StockBase>? stockPortfolio = new List<StockBase>();
             StockBase curstockDetail;
             curstockDetail = new StockBase
             {
                 StockName = "DNB Bank ASA",
                 Symbol = "DNBHF",
                 Type = "Equity",
                 LastUpdated = timeNow,
                 StockCurrency = "USD"
             };
             stockPortfolio.Add(curstockDetail);*/

            List<StockPortfolio>? portStock = new List<StockPortfolio>();
            var currentPortfolio = new StockPortfolio
            {
                Quantity = 3,
                EstPrice = "200",
                PortfolioPortion = "100",
                EstTotalMarketValue = "50.00",
                TotalCost = "1000",
                UnrealizedPL = "-3.00 %",

            };
            portStock.Add(currentPortfolio);
            var convertPortfolio = new Portfolio
            {
                LastUpdate = timeNow,
                TotalValueSpent = "100",
                EstPortfolioValue = "50.00",
                PortfolioCurrency = "USD",
                Stocks = portStock,
                BuyingPower = "10000",
                UnrealizedPL = "-3.00 %"
            };



            // List<Portfolio> stockPortfolio = new List<Portfolio>();
            var stockDetail = new StockOwnerships
            {
                UsersId = 1,
                StocksId = "DNB",
                StockCounter = 3,
                SpentValue = 100
            };
            List<StockOwnerships> mockStocks = new List<StockOwnerships>() { stockDetail };

            //tradingServices.Setup(p => p.CreateCurrentPortfolio(1));
            var tradingService = new TradingService(tradingRepo.Object,logger.Object,serchRepo.Object,config.Object);

            //act
            Portfolio createCurrentPortfolio = await tradingService.CreateCurrentPortfolio(1);
            string obj1 = JsonSerializer.Serialize(currentPortfolio);
            string obj2 = JsonSerializer.Serialize(createCurrentPortfolio);
            // assert
            Assert.Equal(obj1, obj2);

        }

        [Fact]
        public async Task GetAllTradesAsync()
        {
            //arrange
            DateTime timeNow = DateTime.Now;
            var acuelTransaction = new Trades
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
            List<Trades> newTransaction = new List<Trades>() { acuelTransaction };
            var userList = new Users
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
                Saldo = "384,420 NOK",
               

            };
            expectedTransaction.Add(currTransaction);


            

            // arrange
            tradingRepo.Setup(t => t.GetUsersAsync(1)).ReturnsAsync(userList);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);

            // ACT
            List<Trade> getAllTrades = await tradingService.GetAllTradesAsync(1);
            string obj1 = JsonSerializer.Serialize(expectedTransaction);
            string obj2 = JsonSerializer.Serialize(getAllTrades);
            //assert
            Assert.Equal(obj1, obj2);

        }
        [Fact]
        public async Task GetStockQuoteAsync()
        {
            // arrange
            DateTime timeNow = DateTime.Now;

            //List<TradingTrainer.Model.StockQuote>? expStockQute = new List<TradingTrainer.Model.StockQuote>();
           // TradingTrainer.Model.StockQuote creatStockQute;
            var creatStockQute = new TradingTrainer.Model.StockQuote
            {
                Symbol = "DNBBY",
                StockName = "DNB ASA",
                LastUpdated = timeNow,
                Open = "13.66",
                High = "14.08",
                Low = "13.62",
                Price = "13.9",
                Volume = 1526711,
                LatestTradingDay = timeNow,
                PreviousClose = "13.08",
                Change = "0.82",
                ChangePercent = "6.2691%"

            };
            //expStockQute.Add(creatStockQute);


            var stocks = new Stocks
            {
                StockName = "facebook",
                Symbol = "fbc",
                Type = "Equity",
                LastUpdated = timeNow,
                Currency = "USD"
            };
            var actueGetStock = new StockQuotes
            {
                StocksId = "1",
                Stock = stocks,
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
            // List<StockQuotes> stockList = new List<StockQuotes>() { actueGetStock };
            tradingServiceMoq.Setup(q => q.GetUpdatedQuoteAsync("FBC")).ReturnsAsync(actueGetStock);
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);
            //act
            TradingTrainer.Model.StockQuote getStockQute = await tradingService.GetStockQuoteAsync("FBC");
            string obj1 = JsonSerializer.Serialize(creatStockQute);
            string obj2 = JsonSerializer.Serialize(getStockQute);

            // assert
            Assert.Equal(obj1, obj2);

        }

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
            var actualUser = new Users
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
            tradingRepo.Setup(r => r.ResetProfileAsync(1)).ReturnsAsync(actualUser);
            var tradingService = new TradingService(tradingRepo.Object,logger.Object,serchRepo.Object,config.Object);

            User resetportfolio = await tradingService.ResetProfileAsync(1);
            string obj1 = JsonSerializer.Serialize(expectedUser);
            string obj2 = JsonSerializer.Serialize(resetportfolio);
            Assert.Equal(obj1, obj2);

        }




    }


}

