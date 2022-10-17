using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChargeAmpReminder;

public class GmailClient : IGmailClient
{
    private readonly string _gmailUserName;
    private readonly string _emailToNotify;

    private readonly SmtpClient _smtpClient;

    public GmailClient(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;

        _gmailUserName = Environment.GetEnvironmentVariable(Constants.ENV_USER_NAME);
        _emailToNotify = Environment.GetEnvironmentVariable(Constants.ENV_TO_NOTIFY);

        var gmailPassword = Environment.GetEnvironmentVariable(Constants.ENV_GMAIL_USER_PASSWORD);

        _smtpClient.Credentials = new NetworkCredential(_gmailUserName, gmailPassword);
    }

    public async Task SendNotConnectedEmail()
    {
        var email = CreateEmail();

        await _smtpClient.SendMailAsync(email);
    }

    private MailMessage CreateEmail()
    {
        var email = new MailMessage(_gmailUserName, _emailToNotify);
        email.Subject = "Charger not connected";
        email.Body = "Charger not connected";
        return email;
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}