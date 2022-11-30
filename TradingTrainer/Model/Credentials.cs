using System.ComponentModel.DataAnnotations;

namespace TradingTrainer.Model
{
    public class Credentials
    {
        [RegularExpression(@"^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$")]
        public string Username { get; set; }
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*[$&+,:;=?@#|'<>-^*()%!]).{9,32}$")]
        public string Password { get; set; }
        
    }
}
