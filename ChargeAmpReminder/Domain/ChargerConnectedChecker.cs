using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargeAmpReminder.Model;
using ChargeAmpReminder.Model.Api;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder.Domain;

public class ChargerConnectedChecker : IChargerConnectedChecker
{
    private IEnumerable<Status> _currentStatuses;

    private readonly IChargeAmpClient _chargeAmpClient;

    public ChargerConnectedChecker(IChargeAmpClient chargeAmpClient)
    {
        _chargeAmpClient = chargeAmpClient;
    }

    public async Task<ConnectedResult> IsConnected(ILogger log)
    {
        var connectedResult = new ConnectedResult
        {
            Response = new ConnectedResponse(),
            Success = false,
        };
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
                    connectedResult.Success = true;

                    log.LogInformation("Get Status of Charge Point Successful");
                    if (AnyConnectedConnector(response))
                    {
                        log.LogInformation("Charger Connected");

                        var connectedConnector = GetFirstConnectedConnector(response);
                        connectedResult.Response.IsConnected = true;
                        connectedResult.Response.Status = connectedConnector.Status;
                        connectedResult.Response.ConnectorId = connectedConnector.Id;
                    }
                    else
                    {
                        log.LogInformation("No Connected Charger Detected");

                        connectedResult.Response.IsConnected = false;
                    }
                }
                else
                {
                    var message = "Unable to get status";
                    connectedResult.ErrorReason = ErrorReason.ChargeAmpApiFailed;
                    connectedResult.Message = message;
                    log.LogError(message);
                }
            }
            else
            {
                var message = "Unknown protocol";
                connectedResult.ErrorReason = ErrorReason.ChargeAmpApiFailed;
                connectedResult.Message = message;
                log.LogError(message);
            }
        }
        else
        {
            var message = $"Login failed: {loginResult.Message}";
            connectedResult.ErrorReason = ErrorReason.Authorization;
            connectedResult.Message = message;
            log.LogError(message);
        }

        return connectedResult;
    }

    private ConnectorResult GetFirstConnectedConnector(ChargePointStatusResult response)
    {
        return response.ConnectorResults.First(HasConnectedStatus);
    }

    private bool AnyConnectedConnector(ChargePointStatusResult response)
    {
        return response.ConnectorResults.Any(HasConnectedStatus);
    }

    private bool HasConnectedStatus(ConnectorResult result)
    {
        return _currentStatuses.First(s => s.Name.Equals(result.Status, StringComparison.InvariantCultureIgnoreCase))
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
        _chargeAmpClient.Dispose();

        GC.SuppressFinalize(this);
    }
}