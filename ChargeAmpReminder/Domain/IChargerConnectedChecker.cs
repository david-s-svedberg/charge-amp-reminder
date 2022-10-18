using System;
using System.Threading.Tasks;
using ChargeAmpReminder.Model;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder.Domain;

public interface IChargerConnectedChecker : IDisposable
{
    Task<ConnectedResult> IsConnected(ILogger log);
}