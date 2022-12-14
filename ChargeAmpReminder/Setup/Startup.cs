using System.Net.Mail;
using System.Text.Json;
using ChargeAmpReminder.Domain;
using ChargeAmpReminder.Setup;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ChargeAmpReminder.Setup;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        builder.Services.Configure<JsonSerializerOptions>(o =>
        {
            o.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.AddTransient(_ =>
        {
            var smtpClient = new SmtpClient
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Port = 587,
            };

            return smtpClient;
        });

        builder.Services.AddTransient<IChargerConnectedChecker, ChargerConnectedChecker>();
        builder.Services.AddTransient<IGmailClient, GmailClient>();
        builder.Services.AddTransient<IChargeAmpClient, ChargeAmpClient>();
        builder.Services.AddTransient<ISettings, Settings>();
    }
}