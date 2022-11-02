
using System.Text.Json.Serialization;

namespace AlphaVantageInterface.Models  {

    public class StockQuoteTemp
    {
        [JsonPropertyName("Global Quote")]
        public Dictionary<string, dynamic>? GlobalQuote {get; set;}

        public override string ToString()
        {
            if (GlobalQuote is null) {
                GlobalQuote = new Dictionary<string, dynamic>();
            }
            return $"The quote information received {GlobalQuote.ToString()}: \n";
        }
    }

}