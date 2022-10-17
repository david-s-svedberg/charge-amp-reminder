using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ChargeAmpReminder;

public class GmailClient : IGmailClient
{
    private readonly string _gmailUserName;
    private readonly string _emailToNotify;

    private readonly SmtpClient _smtpClient;

    public GmailClient(SmtpClient smtpClient, ISettings settings)
    {
        _smtpClient = smtpClient;

        _gmailUserName = settings.GmailUserName;
        _emailToNotify = settings.EmailToNotify;

        var gmailPassword = settings.GmailPassword;

        _smtpClient.Credentials = new NetworkCredential(_gmailUserName, gmailPassword);
    }

    public async Task SendNotConnectedEmail()
    {
        var email = CreateEmail();

        await _smtpClient.SendMailAsync(email);
    }

    private MailMessage CreateEmail()
    {
        return new MailMessage(_gmailUserName, _emailToNotify)
        {
            Subject = "Charger Not Connected",
            Body = "Charger Not Connected",
        }; ;
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}