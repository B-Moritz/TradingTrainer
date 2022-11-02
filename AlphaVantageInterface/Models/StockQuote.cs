
using System.Text.Json.Serialization;

namespace AlphaVantageInterface.Models  {

    public class StockQuote
    {
        public string Symbol {get; set;}
        public decimal Open {get; set;}
        public decimal High {get; set;}
        public decimal Low {get; set;}
        public decimal Price {get; set;}
        public int Volume {get; set;}
        public string LatestTradingDay {get; set;}
        public decimal PreviousClose {get; set;}
        public decimal Change {get; set;}
        public string ChangePercent {get; set;}

        public override string ToString()
        {
            return $"Stock quote of {Symbol}: \n" + 
                   $"Open: {Open}\nHigh: {High}\nLow: {Low}\nPrice: {Price}\nVolume: {Volume}\n" + 
                   $"Latest trading day: {LatestTradingDay}\nPrevious close: {PreviousClose}\nChange: {Change}\n" + 
                   $"Change percent: {ChangePercent}";
        }
    }

}