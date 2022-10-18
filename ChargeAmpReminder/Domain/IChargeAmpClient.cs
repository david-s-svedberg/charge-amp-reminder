using System;
using System.Threading.Tasks;
using ChargeAmpReminder.Model;

namespace ChargeAmpReminder.Domain;

public interface IChargeAmpClient : IDisposable
{
    Task<ProtocolResult> GetProtocol();

    Task<LoginResult> Login();

    Task<ChargePointStatusResult> GetChargePointStatus();
}