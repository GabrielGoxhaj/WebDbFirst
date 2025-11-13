namespace WebDbFirst.Models
{
    public class Credentials
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        public override string ToString()
        {
            return $"username: {Username}, password: {Password}";
        }
    }
}
