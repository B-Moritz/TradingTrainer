using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TradingTrainer.BLL;
using TradingTrainer.Controllers;
using TradingTrainer.DAL;
using TradingTrainer.Model;
using Xunit;


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
                LastUpdated = timeNow
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
                LastUpdated = timeNow
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
            Assert.Equal(obj1,obj2);
            
        }

        // den er ikke nødvendig å teste nå kanskje om den er interesant kan tas det senere
        [Fact]
        public async Task DeleteFromFavoriteListAsync()
        {
            //arrange
            DateTime timeNow = DateTime.Now;

            List<StockBase>? stockfavorits = new List<StockBase>();
            StockBase curstockDetail;
            curstockDetail = new StockBase
            {
                StockName = "facebook",
                Symbol = "fbc",
                Type = "Equity",
                LastUpdated = timeNow
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
                LastUpdated = timeNow
            };
            List<Stocks> mockStocks = new List<Stocks>() { mockStock };

            tradingRepo.Setup(s => s.GetFavoriteListAsync(1)).ReturnsAsync(mockStocks);

            //tradingRepo.Setup(d => d.DeleteFromFavoriteListAsync(1, "fbc")).
            var tradingService = new TradingService(tradingRepo.Object, logger.Object, serchRepo.Object, config.Object);

            //act
            FavoriteList remove = await tradingService.DeleteFromFavoriteListAsync(1, "fbc");
            remove.LastUpdated = timeNow;
            string obj = JsonSerializer.Serialize(remove);

            // assert
           // Assert.Equal(obj);

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
                FundsAvailable = "1000000",
                FundsSpent = "0",
                Currency = "NOK",
            };


            tradingRepo.Setup(g => g.GetUsersAsync(1)).ReturnsAsync(userMock);
            var tradingService = new TradingService(tradingRepo.Object,logger.Object,serchRepo.Object,config.Object);
            //act
            User getUser = await tradingService.GetUserAsync(1);
            string obj1 = JsonSerializer.Serialize(expectedResultUser);
            string obj2 = JsonSerializer.Serialize(getUser);

            //assert
            Assert.Equal(obj1,obj2);

        }







        [Fact]
        public async Task GetUpdatedQuoteAsync()
        {
            // arrange



        }




        [Fact]
        public async Task Getportfolio_LogetInn_Ok()
        {
            // Arange


            // Act


            // Assert

        }
        [Fact]
        public async Task Getportfolio_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task Getportfolio_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task GetFavoriteList_LogetInn_Ok()
        {


        }
        [Fact]
        public async Task GetFavoriteList_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task GetFavoriteList_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task GetStockQuote_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task GetStockQuote_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task GetStockQuote_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task GetUserSearchResult_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task GetUserSearchResult_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task GetUserSearchResult_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task AddToFavoriteList_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task AddToFavoriteList_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task AddToFavoriteList_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task DeleteFromFavoriteList_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task DeleteFromFavoriteList_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task DeleteFromFavoriteList_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task GetAllTrades_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task GetAllTrades_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task GetAllTrades_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task ClearAllTradeHistory_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task ClearAllTradeHistory_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task ClearAllTradeHistory_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task ResetProfile_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task ResetProfile_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task ResetProfile_IkkeLogetInn()
        {

        }



        [Fact]
        public async Task GetUser_LogetInn_Ok()
        {

        }
        [Fact]
        public async Task GetUser_LogetInn_Feil()
        {

        }
        [Fact]
        public async Task GetUser_IkkeLogetInn()
        {

        }


    }


}

