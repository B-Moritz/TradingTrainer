using System.Text;
using AlphaVantageInterface.Models;
using TradingTrainer.Model;

namespace TradingTrainer.DAL
{
    public interface ITradingRepository
    {
        // Buy, sell and portfolio
        Task<Stocks?> GetStockAsync(string symbol);
        Task SellStockTransactionAsync(int userId, string symbol, decimal saldo, int count);
        Task BuyStockTransactionAsync(Users curUser, Stocks curStock, decimal saldo, int count);
        void RemoveStockQuotes(string symbol);
        Task<StockQuotes> AddStockQuoteAsync(StockQuotes newTableRow);
        Task<StockQuotes?> GetStockQuoteAsync(string symbol);
        // Favorites
        Task<List<Stocks>> GetFavoriteListAsync(int userId);
        Task AddToFavoriteListAsync(int userId, string symbol);
        Task DeleteFromFavoriteListAsync(int userId, string symbol);
        // User
        Task<Users?> GetUsersAsync(int userId);
        Task UpdateUserAsync(User curUser);
        Task<Users> ResetProfileAsync(int userId);
        //Trade history
        Task ClearAllTradeHistoryAsync(int userId);
        Task CleanTradingSchemaAsync();

    }

}
