namespace AlphaVantageInterface.Models {

    public class ConnectionStatus
    {
        public string? ApiKey{ get; set;}
        public DateTime LastCallTime {get; set;}
        public int CallCounter {get; set;}

        public bool IsLimited {get; set;}

        public override string ToString() 
        {
            string Out = String.Format("Alpha Vantage Connection Summary\n" + 
                                       "API key: {0} \n" + 
                                       "Last api call: {1} \n" + 
                                       "Api calls today: {3} \n" + 
                                       "The Api key is limited: {4}", ApiKey, LastCallTime, CallCounter, IsLimited);
            return Out;
        }
    }


}