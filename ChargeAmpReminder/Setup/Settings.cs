using Microsoft.Extensions.Configuration;

namespace ChargeAmpReminder.Setup;

public class Settings : ISettings
{
    private readonly IConfiguration _configuration;

    public Settings(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ChargeAmpUserName => GetSetting(Constants.CHARGE_AMP_USER_NAME_KEY);
    public string ChargeAmpPassword => GetSetting(Constants.CHARGE_AMP_PASSWORD_KEY);
    public string ChargeAmpApiKey => GetSetting(Constants.CHARGE_AMP_API_KEY_KEY);
    public string ChargeAmpChargePointId => GetSetting(Constants.CHARGE_AMP_CHARGE_POINT_ID_KEY);
    public string GmailUserName => GetSetting(Constants.GMAIL_USER_NAME_KEY);
    public string GmailPassword => GetSetting(Constants.GMAIL_USER_PASSWORD_KEY);
    public string EmailToNotify => GetSetting(Constants.EMAIL_TO_NOTIFY_KEY);

    private string GetSetting(string key)
    {
        return _configuration[key];
    }
}