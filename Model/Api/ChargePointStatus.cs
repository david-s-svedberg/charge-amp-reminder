using System.Collections.Generic;

namespace ChargeAmpReminder.Model.Api;

public class ChargePointStatus
{
    public string Id { get; set; }
    public string Status { get; set; }
    public IEnumerable<ConnectorStatus> ConnectorStatuses { get; set; }
}