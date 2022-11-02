namespace Webapplikasjoner_oblig.Model
{
    /***
     * The Trade class models an object containing information about a buy or sell trade.
     * Objects of this type is returned by the GetAllTrades endpoint.
     */
    public class Trade
    {
        public int Id { get; set; }
        public string StockSymbol { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        // legger noen atributer
        public string TransactionType { get; set; }
        public int StockCount { get; set; }
        public string Saldo { get; set; }

    }
}