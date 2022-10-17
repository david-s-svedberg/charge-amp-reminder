namespace ChargeAmpReminder.Model;

public class LoginResult
{
    public bool Success { get; set; }
    public string AuthToken { get; set; }
    public string Message { get; set; }
}