namespace TradingTrainer.Model
{
    /***
     * This class models a basic object used to represent a stock in this web application.
     * The StockQuote, StockPortfolio and StockSearchResult object inherit from this class.
     */
    public class StockBase
    {
        // The unique identifier of a stock
        public string? Symbol { get; set; }
        // The stock name
        public string? StockName { get; set; }
        // The stock type (e.g equity)
        public string? Type { get; set; }
        // The currency that the stock is traded in
        public string? StockCurrency { get; set; }
        // Object create timestamp
        public DateTime LastUpdated { get; set; }
    }
}
