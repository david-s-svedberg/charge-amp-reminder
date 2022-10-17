namespace ChargeAmpReminder;

public interface ISettings
{
    string ChargeAmpUserName { get; }
    string ChargeAmpPassword { get; }
    string ChargeAmpApiKey { get; }
    string ChargeAmpChargePointId { get; }
    string GmailUserName { get; }
    string GmailPassword { get; }
    string EmailToNotify { get; }
}