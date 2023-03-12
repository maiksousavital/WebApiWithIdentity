namespace WebApi.Models
{
    public class LoginResponse
    {
        public bool IsAuthSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Token { get; set; }
    }
}
