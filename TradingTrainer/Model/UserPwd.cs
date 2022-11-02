namespace Webapplikasjoner_oblig.Model
{
    /***
     * A extention to the User class for cases where the password of the user is handeled.
     */
    public class UserPwd : User
    {
        public string Password { get; set; }
    }
}
