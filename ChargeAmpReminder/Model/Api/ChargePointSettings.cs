namespace ChargeAmpReminder.Model.Api;

public class ChargePointSettings
{
    public string Id { get; set; }
    public string Dimmer { get; set; }
    public bool? DownLight { get; set; }
    public float MaxCurrent { get; set; }
}