using Moq;
using TradingTrainer.Controllers;
using TradingTrainer.DAL;
using TradingTrainer.Model;
using Xunit;

namespace TradingTrainerTest
{
    public class TradingControllerTest
    {
        [Fact]
        public async Task GetFavoriteListAsync()
        {
            /**
            // arange 
            List<StockBase>? stockfavorits = new List<StockBase>();
            StockBase curstockDetail;
            foreach(var currstock in stockfavorits)
            {
                curstockDetail = new StockBase
                {
                    StockName = currstock.StockName,
                    Symbol = currstock.Symbol,
                    Type = currstock.Type,
                    LastUpdated = currstock.LastUpdated
                };
                stockfavorits.Add(curstockDetail);
            }
            var curentFavorite = new FavoriteList
            {
                LastUpdated = DateTime.Now,
                StockList = stockfavorits
            };

            var mock = new Mock<ITradingRepository>();
            mock.Setup(s => s.GetFavoriteList()).ReturnsAsync(curentFavorite);
            var tradingController = await new TradingController(mock.Object);

            // act
            List<FavoriteList> result = await tradingController.GetFavoriteList();

            // assert
            Assert.Equal<List<FavoriteList>>(curentFavorite,result);
            */
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

