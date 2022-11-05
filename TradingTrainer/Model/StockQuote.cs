using TradingTrainer.DAL;

namespace TradingTrainer.Model
{
    /***
     * This class models the Stock used to represent the stock quote data. It is inheriting from the StockBase
     * and is sent to the client as a response to calls to GetStockQuote(stymbol).
     */
    public class StockQuote : StockBase
    {
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Price { get; set; }
        public int Volume { get; set; }
        public DateTime LatestTradingDay { get; set; }
        public string PreviousClose { get; set; }
        public string Change { get; set; }
        public string ChangePercent { get; set; }
    }
}
