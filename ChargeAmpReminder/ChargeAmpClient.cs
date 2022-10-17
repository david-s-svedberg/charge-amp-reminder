using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChargeAmpReminder.Model;
using ChargeAmpReminder.Model.Api;

namespace ChargeAmpReminder;

public class ChargeAmpClient : IChargeAmpClient
{
    private readonly string _chargePointUrl;
    private readonly string _chargePointStatusUrl;
    private readonly string _chargeAmpsUserName;
    private readonly string _chargeAmpsPassword;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public ChargeAmpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _chargeAmpsUserName = Environment.GetEnvironmentVariable(Constants.ENV_CHARGE_AMP_USER_NAME);
        _chargeAmpsPassword = Environment.GetEnvironmentVariable(Constants.ENV_CHARGE_AMP_PASSWORD);
        _apiKey = Environment.GetEnvironmentVariable(Constants.ENV_CHARGE_AMP_API_KEY);

        var chargePointId = Environment.GetEnvironmentVariable(Constants.ENV_CHARGE_AMP_CHARGE_POINT_ID);
        _chargePointUrl = $"{Constants.CHARGE_POINTS_URL}/{chargePointId}";
        _chargePointStatusUrl = $"{_chargePointUrl}/status";
    }

    public async Task<ProtocolResult> GetProtocol()
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
            protocolResult.Protocol = Protocol.Unknown;
            protocolResult.Success = false;
        }

        return protocolResult;
    }

    private async Task<Protocol> GetOwnedProtocol()
    {
        var ownedProtocol = Protocol.Unknown;
        var response = await _httpClient.GetAsync(Constants.OWNED_CHARGE_POINTS_URL);
        if (response.IsSuccessStatusCode)
        {
            var ownedChargePointResponse = await response.Content.ReadFromJsonAsync<IEnumerable<ChargePoint>>();

            var chargePoint = ownedChargePointResponse?.First();
            if (chargePoint?.OcppVersion == null)
            {
                ownedProtocol = Protocol.Capi;
            }
            else if (chargePoint.OcppVersion.Equals("1.6"))
            {
                ownedProtocol = Protocol.Ocpp;
            }
        }

        return ownedProtocol;
    }

    private async Task<Protocol> GetChargePointProtocol()
    {
        Protocol chargePointProtocol = Protocol.Unknown;
        var response = await _httpClient.GetAsync(_chargePointUrl);
        if (response.IsSuccessStatusCode)
        {
            var chargePointResponse = await response.Content.ReadFromJsonAsync<ChargePoint>();

            if (chargePointResponse?.OcppVersion == null)
            {
                chargePointProtocol = Protocol.Capi;
            }
            else if (chargePointResponse.OcppVersion.Equals("1.6"))
            {
                chargePointProtocol = Protocol.Ocpp;
            }
        }

        return chargePointProtocol;
    }

    private void AddAuthorizationHeader(LoginResult loginResult)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AuthToken);
    }

    public async Task<LoginResult> Login()
    {
        var loginResult = new LoginResult();
        var loginBody = new { email = _chargeAmpsUserName, password = _chargeAmpsPassword };
        var loginRequest = JsonContent.Create(loginBody);
        loginRequest.Headers.Add(Constants.API_KEY_HEADER_KEY, _apiKey);
        var response = await _httpClient.PostAsJsonAsync(Constants.LOGIN_URL, loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse != null)
            {
                loginResult.AuthToken = loginResponse.Token;
                loginResult.Success = true;

                AddAuthorizationHeader(loginResult);
            }
            else
            {
                loginResult.Success = false;
                loginResult.Message = "Bad format";
            }
        }
        else
        {
            loginResult.Success = false;
            loginResult.Message = response.ReasonPhrase;
        }

        return loginResult;
    }

    public async Task<ChargePointStatusResult> GetChargePointStatus()
    {
        var result = new ChargePointStatusResult();

        var chargePointStatusResponse = await _httpClient.GetFromJsonAsync<ChargePointStatus>(_chargePointStatusUrl);

        if (chargePointStatusResponse != null)
        {
            result.ChargePointStatus = chargePointStatusResponse.Status;
            result.ConnectorStatuses = chargePointStatusResponse.ConnectorStatuses.Select(c => c.Status).ToArray();
            result.Success = true;
        }
        else
        {
            result.Success = false;
        }

        return result;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}