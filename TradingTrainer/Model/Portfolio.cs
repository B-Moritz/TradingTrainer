namespace TradingTrainer.Model
{
    /***
     * This class models the Portfolio object containsing a summary of a users position. 
     * I consists of a list of owned stocks (Stocks), the currency of the portfolio, 
     * total value put into the stocks (TotalValueSpent), the total estimated value 
     * of the portfolio (EstPortfolioValue), BuyingPower and Unrealized Profit/Loss. 
     * The LasUpdate contains the timestamp of when the portfolio object was created.
     */
    public class Portfolio
    {
        // Object create timestamp
        public DateTime LastUpdate { get; set; }
        public string TotalValueSpent { get; set; }
        // The estimated market value of the portfolio
        public string EstPortfolioValue { get; set; }
        // The currency used to show the values
        public string PortfolioCurrency { get; set; }
        // The portfolio stocks
        public List<StockPortfolio> Stocks { get; set; }
        // The total funds available to use by the user
        public string BuyingPower { get; set; }

        // The unrealized profit & loss for the entire portfolio
        public string UnrealizedPL { get; set; }
    }
}
