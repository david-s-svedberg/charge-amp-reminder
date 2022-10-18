namespace ChargeAmpReminder.Model;

public class ConnectedResult
{
    public bool Success { get; set; }
    public ConnectedResponse Response { get; set; }
    public ErrorReason ErrorReason { get; set; }
    public string Message { get; set; }
}

public enum ErrorReason
{
    None,
    Authorization,
    ChargeAmpApiFailed,
}

public class ConnectedResponse
{
    public bool IsConnected { get; set; }
    public int ConnectorId { get; set; }
    public string Status { get; set; }
}