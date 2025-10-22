namespace DataService.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime RegT { get; set; }
    public List<Rating> UsersRatings { get; set; }
}
