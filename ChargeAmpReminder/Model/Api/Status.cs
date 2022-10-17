namespace ChargeAmpReminder.Model.Api;

public class Status
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