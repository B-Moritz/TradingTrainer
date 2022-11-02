namespace Webapplikasjoner_oblig.Model
{
    /***
     * This class models the stock objects that should be added to the StockList property of the SearchResult objects
     * The IsFavorite prpoerty is a flag that indicates if the stock is in the favorite list of the user that executed 
     * the search.
     */
    public class StockSearchResult : StockBase 
    {
        public bool IsFavorite { get; set; }
    }
}
