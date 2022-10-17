using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargeAmpReminder.Model;
using ChargeAmpReminder.Model.Api;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder
{
    public class ChargeMonitor : IDisposable
    {
        private IEnumerable<Status> _currentStatuses;

        private readonly IChargeAmpClient _chargeAmpClient;
        private readonly IGmailClient _gmailClient;

        public ChargeMonitor(IGmailClient gmailClient, IChargeAmpClient chargeAmpClient)
        {
            _gmailClient = gmailClient;
            _chargeAmpClient = chargeAmpClient;
        }

        [FunctionName("ChargeMonitor")]
        public async Task Run([TimerTrigger("0 21 * * *")] TimerInfo myTimer, ILogger log)
        {
            var loginResult = await _chargeAmpClient.Login();

            if (loginResult.Success)
            {
                log.LogInformation("Login to Charge Amps Successful");
                var protocolResult = await SetStatusesFromProtocol();

                if (protocolResult.Success)
                {
                    log.LogInformation("Determining Protocol Successful");
                    var response = await _chargeAmpClient.GetChargePointStatus();
                    if (response.Success)
                    {
                        log.LogInformation("Get Status of Charge Point Successful");
                        if (response.ConnectorStatuses.None(IsConnectedStatus))
                        {
                            log.LogInformation("No Connected Charger Detected, Sending Email");
                            await _gmailClient.SendNotConnectedEmail();
                        }
                        else
                        {
                            log.LogInformation("Charger Connected");
                        }
                    }
                    else
                    {
                        log.LogError("Unable to get status");
                    }
                }
                else
                {
                    log.LogError("Unknown protocol");
                }
            }
            else
            {
                log.LogError("Login failed: {}", loginResult.Message);
            }
        }

        private bool IsConnectedStatus(string status)
        {
            return _currentStatuses.First(s => s.Name.Equals(status, StringComparison.InvariantCultureIgnoreCase))
                .Connected;
        }

        private async Task<ProtocolResult> SetStatusesFromProtocol()
        {
            var protocolResult = await _chargeAmpClient.GetProtocol();

            _currentStatuses = protocolResult.Protocol switch
            {
                Protocol.Capi => Constants.CapiStatuses,
                Protocol.Ocpp => Constants.OcppStatuses,
                _ => _currentStatuses
            };

            return protocolResult;
        }

        public void Dispose()
        {
            _gmailClient.Dispose();
            _chargeAmpClient.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}