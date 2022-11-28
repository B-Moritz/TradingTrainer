using System.ComponentModel.DataAnnotations;

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
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ. \-]{2,20}$")]
        public string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ. \-]{2,20}$")]
        public string LastName { get; set; }
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
        public string Email { get; set; }
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$")]
        // password needs minimum eight characters, and at least one letter and one number
        public string Password { get; set; }

        public string AlphaVantageApiKey { get; set; }
        // The buying power of the user
        public string FundsAvailable { get; set; }
        // The total amount of funds that have been invested by the user since the last reset.
        public string FundsSpent { get; set; }
        public string Currency { get; set; }
    }
}
