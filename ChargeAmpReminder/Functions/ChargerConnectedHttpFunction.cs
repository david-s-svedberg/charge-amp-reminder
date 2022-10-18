using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ChargeAmpReminder.Domain;
using ChargeAmpReminder.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder.Functions
{
    public class ChargerConnectedHttpFunction
    {
        private readonly IChargerConnectedChecker _chargerConnectedChecker;

        public ChargerConnectedHttpFunction(IChargerConnectedChecker chargerConnectedChecker)
        {
            _chargerConnectedChecker = chargerConnectedChecker;
        }

        [FunctionName(nameof(ChargerConnectedHttpFunction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var connectedResult = await _chargerConnectedChecker.IsConnected(log);

            var result = connectedResult.Success
                ? new OkObjectResult(connectedResult.Response)
                : CreateErrorResponse(connectedResult);

            return result;
        }

        private static IActionResult CreateErrorResponse(ConnectedResult connectedResult)
        {
            var errorResponse = new HttpResponseMessage
            {
                Content = new StringContent(connectedResult.Message),
                StatusCode = connectedResult.ErrorReason switch
                {
                    ErrorReason.Authorization => HttpStatusCode.Unauthorized,
                    ErrorReason.ChargeAmpApiFailed => HttpStatusCode.InternalServerError,
                    _ => throw new ArgumentOutOfRangeException(nameof(connectedResult))
                },
            };

            return new ResponseMessageResult(errorResponse);
        }
    }
}