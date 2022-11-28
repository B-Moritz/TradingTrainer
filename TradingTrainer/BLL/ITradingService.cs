using TradingTrainer.DAL;
using TradingTrainer.Model;
using TradingTrainer.BLL;
using AlphaVantageInterface.Models;

namespace TradingTrainer.BLL
{
    public interface ITradingService
    {
        Task<Portfolio> CreateCurrentPortfolio(int userId);


        // Search service methods
        Task<Model.SearchResult> CreateOneSearchResult(string keyword);
        Task<Model.SearchResult> CreateUserSearchResult(string keyword, int userId);
        Task<List<Model.SearchResult>> GetAllSearchResults();

        // Favorites service methods
        Task<FavoriteList> CreateFavoriteListAsync(int userId);
        Task<FavoriteList> DeleteFromFavoriteListAsync(int userId, string symbol);
        Task<FavoriteList> AddToFavoriteListAsync(int userId, string symbol);

        // Buy/sell stocks services
        Task BuyStock(int userId, string symbol, int count);
        Task SellStock(int userId, string symbol, int count);
        Task<Model.StockQuote> GetStockQuoteAsync(string symbol);

        // Trades 
        Task<List<Trade>> GetAllTradesAsync(int userId);
        Task ClearAllTradeHistoryAsync(int userId);

        // User services
        Task<User> GetUserAsync(int userId);
        Task<User> GetUserAsync(string username);
        Task<User> UpdateUserAsync(User curUser);
        Task<User> ResetProfileAsync(int userId);
        StockQuotes CreateNewStockQuoteEntity(AlphaVantageInterface.Models.StockQuote stockQuote);
        Task<StockQuotes> GetUpdatedQuoteAsync(string symbol);
        //Task<AlphaVantageInterface.Models.StockQuote> CreateNewStockQuoteEntity(AlphaVantageInterface.Models.StockQuote symbol);
        Task<bool> CreateUser(User user);

    }
}
