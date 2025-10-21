namespace DataService.Entities;

public abstract class UserAction
{
    public User user { get; set; }
    public DateTime time { get; set; }
}
