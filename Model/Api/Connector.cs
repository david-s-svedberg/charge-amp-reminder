using System.Collections.Generic;

namespace ChargeAmpReminder.Model.Api;

public class Connector
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string Type { get; set; }
    public IEnumerable<ConnectorSettings> Settings { get; set; }
}