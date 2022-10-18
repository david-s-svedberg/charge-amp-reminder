using System.Collections.Generic;
using ChargeAmpReminder.Model.Api;

namespace ChargeAmpReminder;

public static class Constants
{
    public const string CHARGE_AMP_USER_NAME_KEY = "charge_amp_user_name";
    public const string CHARGE_AMP_PASSWORD_KEY = "charge_amp_password";
    public const string CHARGE_AMP_API_KEY_KEY = "charge_amp_apiKey";
    public const string GMAIL_USER_NAME_KEY = "gmail_user_name";
    public const string EMAIL_TO_NOTIFY_KEY = "email_to_notify";
    public const string GMAIL_USER_PASSWORD_KEY = "gmail_user_password";
    public const string CHARGE_AMP_CHARGE_POINT_ID_KEY = "charge_amp_chargePointId";
    public const string SCHEDULE_EXPRESSION_KEY = "%schedule_expression%";

    public const string BASE_URL = @"https://eapi.charge.space/api/v4";
    public const string API_KEY_HEADER_KEY = "apiKey";

    public const string LOGIN_URL = @$"{BASE_URL}/auth/login";
    public const string CHARGE_POINTS_URL = @$"{BASE_URL}/chargepoints";
    public const string OWNED_CHARGE_POINTS_URL = @$"{CHARGE_POINTS_URL}/owned";

    public static readonly IEnumerable<Status> OcppStatuses = new Status[]
    {
        new(0, "Available", false),
        new(1, "Preparing", true),
        new(2, "Charging", true),
        new(3, "SuspendedEVSE", true),
        new(4, "SuspendedEV", true),
        new(5, "Finishing", false),
        new(6, "Reserved", false),
        new(7, "Unavailable", false),
        new(8, "Faulted", true),
        new(9, "Unknown", false),
    };

    public static readonly IEnumerable<Status> CapiStatuses = new Status[]
    {
        new (0, "Available", false),
        new (1, "Charging", true),
        new (2, "Connected", true),
        new (3, "Error", false),
        new (4, "Unknown", false),
    };
}