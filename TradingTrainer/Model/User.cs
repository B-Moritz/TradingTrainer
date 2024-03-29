﻿using System.ComponentModel.DataAnnotations;

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
        [RegularExpression(@"^[\w\s]{2,50}$")]
        public string? FirstName { get; set; }
        [RegularExpression(@"^[\w\s]{2,50}$")]
        public string? LastName { get; set; }
        [RegularExpression(@"^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$")]
        public string? Email { get; set; }
        // Backup løsning [StringLength(30, MinimumLength = 9, ErrorMessage = "The password must be between 9 and 30 characters")]
        // password needs minimum eight characters, and at least one letter and one number
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*[$&+,:;=?@#|'<>-^*()%!]).{9,32}$")]
        public string? Password { get; set; }

        [StringLength(30, MinimumLength = 1, ErrorMessage = "The Alpha Vantage api key must be less than 30 characters long.")]
        public string? AlphaVantageApiKey { get; set; }
        // The buying power of the user
        public string? FundsAvailable { get; set; }
        // The total amount of funds that have been invested by the user since the last reset.
        public string? FundsSpent { get; set; }

        public string? Currency { get; set; }
    }
}
