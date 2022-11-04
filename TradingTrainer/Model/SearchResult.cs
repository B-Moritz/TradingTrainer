namespace TradingTrainer.Model
{
    /***
     * This class models the Search result objects used to contains a list of sotcks that match the search keyword.
     * The object is used to respond to GetUserSearchResult requests.
     */
    public class SearchResult
    {
        public string SearchKeyword { get; set; }
        public DateTime SearchTime { get; set; }
        public List<StockSearchResult> StockList { get; set; }

    }
}
