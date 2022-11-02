namespace Webapplikasjoner_oblig.Model
{
    /***
     * This class models the objects that should contian the favorite lists of each user. 
     * Each user has one list of stocks. The LastUpdated attribute contains the timestamp for
     * when the object was created.
     */
    public class FavoriteList
    {
        public DateTime LastUpdated { get; set; }
        public List<StockBase> StockList { get; set; }
    }
}
