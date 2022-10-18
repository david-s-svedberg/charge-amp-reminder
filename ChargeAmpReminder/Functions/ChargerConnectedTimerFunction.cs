using System.Threading.Tasks;
using ChargeAmpReminder.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder.Functions;

public class ChargerConnectedTimerFunction
{
    private const string CRON_TRIGGER = "0 21 * * *";

    private readonly IChargerConnectedChecker _chargerConnectedChecker;
    private readonly IGmailClient _gMailClient;

    public ChargerConnectedTimerFunction(IChargerConnectedChecker chargerConnectedChecker, IGmailClient gMailClient)
    {
        _chargerConnectedChecker = chargerConnectedChecker;
        _gMailClient = gMailClient;
    }

    [FunctionName(nameof(ChargerConnectedTimerFunction))]
    public async Task Run([TimerTrigger(CRON_TRIGGER)] TimerInfo myTimer, ILogger log)
    {
        var result = await _chargerConnectedChecker.IsConnected(log);
        if (result.Success && !result.Response.IsConnected)
        {
            log.LogInformation("Sending Email");
            await _gMailClient.SendNotConnectedEmail();
        }
    }
}