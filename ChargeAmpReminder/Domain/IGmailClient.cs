using System;
using System.Threading.Tasks;

namespace ChargeAmpReminder.Domain;

public interface IGmailClient : IDisposable
{
    Task SendNotConnectedEmail();
}