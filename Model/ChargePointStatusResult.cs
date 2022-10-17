using System.Collections.Generic;

namespace ChargeAmpReminder.Model;

public class ChargePointStatusResult
{
    public bool Success { get; set; }
    public string ChargePointStatus { get; set; }
    public IEnumerable<string> ConnectorStatuses { get; set; }
}