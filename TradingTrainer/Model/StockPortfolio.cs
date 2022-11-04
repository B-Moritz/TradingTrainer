namespace TradingTrainer.Model
{
    /***
     * This class models a stock that the user owns. The object is added to the Stocks list in Portfolio objects.
     * The class inherits from the StockBase class (stock symbol, name, description).
     */
    public class StockPortfolio : StockBase
    {
        // Number of stocks owned by the user
        public int Quantity { get; set; }
        // Estimated price of the stock. This value is value is from the
        // Quote endpoint in the Alpha Vantage API.
        public string? EstPrice { get; set; }
        // The relative amount of the stock in the portfolio.
        public string? PortfolioPortion { get; set; }
        // Estimated total market value of the stock position owned.
        // Based on the estimated price value.
        public string? EstTotalMarketValue { get; set; }
        // The total funds spent on the stock.
        public string? TotalCost { get; set; }
        // The difference between current market value and the total cost
        public string? UnrealizedPL { get; set; }
    }
}
