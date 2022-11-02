using System.Text.Json.Serialization;

namespace AlphaVantageInterface.Models {

    public class SearchResult 
    {
        [JsonPropertyName("bestMatches")]
        public List<Stock>? BestMatches {get; set;}


        public override string ToString() 
        {
            string outString = "Search Result: \n";
            
            if (BestMatches == null)  {
                return "The search result is empy";
            }

            for (int i = 1; i <= BestMatches.Count; i++) {
                outString += String.Format("Stock {0}: \n{1}", i, this.BestMatches[i-1].ToString());
            }
            return outString;
        }
    }

}