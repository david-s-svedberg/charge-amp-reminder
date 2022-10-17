namespace ChargeAmpReminder.Model.Api;

public class ConnectorSettings
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public float MaxCurrent { get; set; }
    public bool RfidLock { get; set; }
    public string Mode { get; set; }
    public bool CableLock { get; set; }
}