using System.Collections.Generic;

namespace ChargeAmpReminder.Model.Api;

public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public IEnumerable<RfidTag> RfidTags { get; set; }
    public string UserStatus { get; set; }
}