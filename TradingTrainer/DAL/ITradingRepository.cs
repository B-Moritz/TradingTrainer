using System.Text;
using AlphaVantageInterface.Models;
using TradingTrainer.Model;

namespace TradingTrainer.DAL
{
    public interface ITradingRepository
    {
        Task<List<Trade>> GetAllTradesAsync(int userId);
        Task<Stocks?> GetStockAsync(string symbol);
        Task<StockQuotes?> GetStockQuoteAsync(string symbol);

        Task SellStockTransactionAsync(int userId, string symbol, decimal saldo, int count);
        Task BuyStockTransactionAsync(Users curUser, Stocks curStock, decimal saldo, int count);
        void RemoveStockQuotes(string symbol);

        Task<User> GetUserAsync(int userId);
        Task<StockQuotes> AddStockQuoteAsync(AlphaVantageInterface.Models.StockQuote stockQuote);
        Task<List<Stocks>> GetFavoriteListAsync(int userId);
        Task AddToFavoriteListAsync(int userId, string symbol);
        Task DeleteFromFavoriteListAsync(int userId, string symbol);

        Task<Users?> GetUsersAsync(int userId);

        Task<User> ResetProfile(int userId);

        Task UpdateUserAsync(User curUser);

        Task ClearAllTradeHistoryAsync(int userId);

        Task CleanTradingSchemaAsync();

    }

}
