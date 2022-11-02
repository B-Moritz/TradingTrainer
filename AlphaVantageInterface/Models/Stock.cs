using System.Text.Json.Serialization;

namespace AlphaVantageInterface.Models {

    public class Stock 
    {
        [JsonPropertyName("1. symbol")]
        public string? Symbol {get; set;}
        
        [JsonPropertyName("2. name")]
        public string? Name {get; set;}

        [JsonPropertyName("3. type")]
        public string? Type {get; set;}

        [JsonPropertyName("4. region:")]
        public string? Region {get; set;}

        [JsonPropertyName("5. marketOpen")]
        public string? MarketOpen {get; set;}

        [JsonPropertyName("6. marketClose")]
        public string? MarketClose {get; set;}

        [JsonPropertyName("7. timezone")]
        public string? Timezone {get; set;}

        [JsonPropertyName("8. currency")]
        public string? Currency {get; set;}

        [JsonPropertyName("9. matchScore")]
        public float MatchScore {get; set;}

        public StockQuote? StockQuoteObject {get; set;}

        public override string ToString() 
        {
            if (Name == null) {
                Name = "Not found";
            }
            if (Symbol == null) {
                Symbol = "Not found";
            }
            string outString = String.Format("   Stock Symbol: {0, 30}\n   Stock name: {1, 30}\n", this.Symbol, this.Name);
            return outString;
        }
    }

}