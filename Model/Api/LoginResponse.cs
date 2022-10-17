namespace ChargeAmpReminder.Model.Api;

public class LoginResponse
{
    public string Message { get; set; }
    public string Token { get; set; }
    public User User { get; set; }
    public string RefreshToken { get; set; }
}