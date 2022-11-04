namespace TradingTrainer.Model
{
    /***
     * This class models the User object containing information about a user of this application. 
     * Objects of this class are sent as response after requests to GetUser and UpdateUser endpoints
     */
    public class User
    {
        // UsersId
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        // The buying power of the user
        public string FundsAvailable { get; set; }
        // The total amount of funds that have been invested by the user since the last reset.
        public string FundsSpent { get; set; }

        public string Currency { get; set; }
    }
}
