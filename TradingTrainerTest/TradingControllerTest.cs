
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


namespace TradingTrainerTest
{
    public class TradingControllerTest
    {
        private TradingService tradingService;
        private Mock<ILogger<TradingService>> logger;
        private Mock<ISearchResultRepositry> serchRepo;
        private Mock<IConfiguration> config;
        private Mock<ITradingRepository> tradingRepo;


        public TradingControllerTest()
        {
            logger = new Mock<ILogger<TradingService>>();
            serchRepo = new Mock<ISearchResultRepositry>();
            config = new Mock<IConfiguration>();
            tradingRepo = new Mock<ITradingRepository>();

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
            var tradingService = new TradingService(tradingRepo.Object,logger.Object, serchRepo.Object,config.Object);

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
            var tradingService = new TradingService(tradingRepo.Object,logger.Object,serchRepo.Object,config.Object);
            //act
            User getUser = await tradingService.GetUserAsync(1);
            string obj11 = JsonSerializer.Serialize(expectedResultUser);
            string obj22 = JsonSerializer.Serialize(getUser);

            //assert
            Assert.Equal(obj11,obj22);

        }
        [Fact]
        public async Task CreateNewStockQuoteEntity()
        {
            // arrange


        }



    }


}

