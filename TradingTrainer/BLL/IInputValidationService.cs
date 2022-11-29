using System;

namespace TradingTrainer.BLL
{
    public interface IInputValidationService
    {
        bool ValidateStockCount(int count);
        bool ValidateStockSymbol(string symbol);
        
        bool ValidateUserId(int userId);

        bool ValidateSearchKeyword(string keyword);

        bool ValidateUsername(string username);
    }
}
