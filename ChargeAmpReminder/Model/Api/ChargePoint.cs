using System.Collections.Generic;

namespace ChargeAmpReminder.Model.Api;

public class ChargePoint
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Type { get; set; }
    public bool IsLoadbalanced { get; set; }
    public string FirmwareVersion { get; set; }
    public string HardwareVersion { get; set; }
    public string OcppVersion { get; set; }
    public IEnumerable<ChargePointSettings> Settings { get; set; }
    public IEnumerable<Connector> Connectors { get; set; }
}