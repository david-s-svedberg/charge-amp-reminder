using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ChargeAmpReminder
{
    public class ChargeMonitor
    {
        private const string BaseUrl = @"https://eapi.charge.space/api/v4";
        private const string LoginUrl = @$"{BaseUrl}/auth/login";
        private const string ChargePointsUrl = @$"{BaseUrl}/chargepoints";
        private const string OwnedChargePointsUrl = @$"{ChargePointsUrl}/owned";

        private const string API_KEY_HEADER_KEY = "apiKey";

        private readonly string _chargePointUrl;
        private readonly string _chargePointStatusUrl;

        private readonly IEnumerable<Status> ocppStatuses = new Status[]
        {
            new(0, "Available", false),
            new(1, "Preparing", true),
            new(2, "Charging", true),
            new(3, "SuspendedEVSE", true),
            new(4, "SuspendedEV", true),
            new(5, "Finishing", false),
            new(6, "Reserved", false),
            new(7, "Unavailable", false),
            new(8, "Faulted", true),
            new(9, "Unknown", false),
        };

        private readonly IEnumerable<Status> capiStatuses = new Status[]
        {
            new (0, "Available", false),
            new (1, "Charging", true),
            new (2, "Connected", true),
            new (3, "Error", false),
            new (4, "Unknown", false),
        };

        private readonly IDictionary<int, string> _connectedStatuses;

        private IEnumerable<Status> _currentStatuses;

        private readonly string _email;
        private readonly string _password;

        private readonly string _apiKey;

        private readonly HttpClient _httpClient;

        public ChargeMonitor(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();

            _email = Environment.GetEnvironmentVariable("charge_amp_email");
            _password = Environment.GetEnvironmentVariable("charge_amp_password");
            _apiKey = Environment.GetEnvironmentVariable("charge_amp_apiKey");
            var chargePointId = Environment.GetEnvironmentVariable("charge_amp_chargePointId");

            _chargePointUrl = $"{ChargePointsUrl}/{chargePointId}";
            _chargePointStatusUrl = $"{_chargePointUrl}/status";
        }

        [FunctionName("ChargeMonitor")]
        public async Task Run([TimerTrigger("0 21 * * *")] TimerInfo myTimer, ILogger log)
        {
            var loginResult = await Login();

            if (loginResult.Success)
            {
                AddAuthorizationHeader(loginResult);

                var protocolResult = await SetStatusesFromProtocol();

                if (protocolResult.Success)
                {
                    var response = await _httpClient.GetAsync(_chargePointStatusUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var chargePointStatusResponse = (await response.Content.ReadFromJsonAsync<ChargePointStatus>());
                        if (chargePointStatusResponse != null)
                        {
                            log.LogDebug("chargePointStatus:'{}'", chargePointStatusResponse.status);
                            foreach (var connectorStatus in chargePointStatusResponse.connectorStatuses)
                            {
                                log.LogDebug("Connector:'{}', Status:'{}'", connectorStatus.connectorId,
                                    connectorStatus.status);
                            }

                            if (chargePointStatusResponse.connectorStatuses.None(s => IsConnectedStatus(s.status)))
                            {
                            }
                        }
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
            var protocolResult = await GetProtocol();

            if (protocolResult.Protocol == Protocol.CAPI)
            {
                _currentStatuses = capiStatuses;
            }
            else if (protocolResult.Protocol == Protocol.OCPP)
            {
                _currentStatuses = ocppStatuses;
            }

            return protocolResult;
        }

        private async Task<ProtocolResult> GetProtocol()
        {
            var protocolResult = new ProtocolResult();

            var chargePointProtocol = await GetChargePointProtocol();
            var ownedProtocol = await GetOwnedProtocol();

            if (chargePointProtocol == ownedProtocol)
            {
                protocolResult.Protocol = chargePointProtocol;
                protocolResult.Success = true;
            }
            else
            {
                protocolResult.Protocol = Protocol.UNKNOWN;
                protocolResult.Success = false;
            }

            return protocolResult;
        }

        private async Task<Protocol> GetOwnedProtocol()
        {
            var ownedProtocol = Protocol.UNKNOWN;
            var response = await _httpClient.GetAsync(OwnedChargePointsUrl);
            if (response.IsSuccessStatusCode)
            {
                var ownedChargePointResponse = await response.Content.ReadFromJsonAsync<IEnumerable<ChargePoint>>();

                var chargePoint = ownedChargePointResponse?.First();
                if (chargePoint?.ocppVersion == null)
                {
                    ownedProtocol = Protocol.CAPI;
                }
                else if (chargePoint.ocppVersion.Equals("1.6"))
                {
                    ownedProtocol = Protocol.OCPP;
                }
            }

            return ownedProtocol;
        }

        private async Task<Protocol> GetChargePointProtocol()
        {
            Protocol chargePointProtocol = Protocol.UNKNOWN;
            var response = await _httpClient.GetAsync(_chargePointUrl);
            if (response.IsSuccessStatusCode)
            {
                var chargePointResponse = await response.Content.ReadFromJsonAsync<ChargePoint>();

                if (chargePointResponse?.ocppVersion == null)
                {
                    chargePointProtocol = Protocol.CAPI;
                }
                else if (chargePointResponse.ocppVersion.Equals("1.6"))
                {
                    chargePointProtocol = Protocol.OCPP;
                }
            }

            return chargePointProtocol;
        }

        private void AddAuthorizationHeader(LoginResult loginResult)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AuthToken);
        }

        private async Task<LoginResult> Login()
        {
            var loginResult = new LoginResult();
            var loginBody = new { email = _email, password = _password };
            var loginRequest = JsonContent.Create(loginBody);
            loginRequest.Headers.Add(API_KEY_HEADER_KEY, _apiKey);
            var response = await _httpClient.PostAsJsonAsync(LoginUrl, loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                loginResult.AuthToken = loginResponse.token;
                loginResult.Success = true;
            }
            else
            {
                loginResult.Success = false;
                loginResult.Message = response.ReasonPhrase;
            }

            return loginResult;
        }

        private class Status
        {
            public Status(int id, string name, bool connected)
            {
                Id = id;
                Name = name;
                Connected = connected;
            }

            public int Id { get; }
            public string Name { get; }
            public bool Connected { get; }
        }

        private class Measurement
        {
            public string phase { get; set; }
            public float current { get; set; }
            public float voltage { get; set; }
        }

        private class ConnectorStatus
        {
            public string chargePointId { get; set; }
            public int connectorId { get; set; }
            public double totalConsumptionKwh { get; set; }
            public string sessionType { get; set; }
            public string status { get; set; }
            public IEnumerable<Measurement> measurements { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
            public int sessionId { get; set; }
        }

        private class ChargePointStatus
        {
            public string id { get; set; }
            public string status { get; set; }
            public IEnumerable<ConnectorStatus> connectorStatuses { get; set; }
        }

        private class ChargePointSettings
        {
            public string id { get; set; }
            public string dimmer { get; set; }
            public bool? downLight { get; set; }
            public float maxCurrent { get; set; }
        }

        private class ConnectorSettings
        {
            public string chargePointId { get; set; }
            public int connectorId { get; set; }
            public float maxCurrent { get; set; }
            public bool rfidLock { get; set; }
            public string mode { get; set; }
            public bool cableLock { get; set; }
        }

        private class Connector
        {
            public string chargePointId { get; set; }
            public int connectorId { get; set; }
            public string type { get; set; }
            public IEnumerable<ConnectorSettings> settings { get; set; }
        }

        private class ChargePoint
        {
            public string id { get; set; }
            public string name { get; set; }
            public string password { get; set; }
            public string type { get; set; }
            public bool isLoadbalanced { get; set; }
            public string firmwareVersion { get; set; }
            public string hardwareVersion { get; set; }
            public string ocppVersion { get; set; }
            public IEnumerable<ChargePointSettings> settings { get; set; }
            public IEnumerable<Connector> connectors { get; set; }
        }

        private class RfidTag
        {
            public bool active { get; set; }
            public string rfid { get; set; }
            public string rfidDec { get; set; }
            public string rfidDecReverse { get; set; }
        }

        private class User
        {
            public string id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string mobile { get; set; }
            public IEnumerable<RfidTag> rfidTags { get; set; }
            public string userStatus { get; set; }
        }

        private class LoginResponse
        {
            public string message { get; set; }
            public string token { get; set; }
            public User user { get; set; }
            public string refreshToken { get; set; }
        }

        private class LoginResult
        {
            public bool Success { get; set; }
            public string AuthToken { get; set; }
            public string Message { get; set; }
        }

        private class ProtocolResult
        {
            public Protocol Protocol { get; set; }
            public bool Success { get; set; }
        }

        private enum Protocol
        {
            UNKNOWN,
            CAPI,
            OCPP,
        }
    }
}