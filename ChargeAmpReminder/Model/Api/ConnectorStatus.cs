using System;
using System.Collections.Generic;

namespace ChargeAmpReminder.Model.Api;

public class ConnectorStatus
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public double TotalConsumptionKwh { get; set; }
    public string SessionType { get; set; }
    public string Status { get; set; }
    public IEnumerable<Measurement> Measurements { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? SessionId { get; set; }
}