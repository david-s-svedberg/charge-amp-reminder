using System;
using System.Threading.Tasks;

namespace ChargeAmpReminder;

public interface IGmailClient : IDisposable
{
    Task SendNotConnectedEmail();
}