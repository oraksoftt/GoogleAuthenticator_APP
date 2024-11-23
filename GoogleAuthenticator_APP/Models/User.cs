namespace GoogleAuthenticator_APP.Models
{
    public class User2FA
    {

        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool Is2FAEnabled { get; set; }
        public int KeyIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


    }
    public class Request
    {
        public string Code { get; set; } = string.Empty;
        public int KeyIndex { get; set; } 


    }
}
