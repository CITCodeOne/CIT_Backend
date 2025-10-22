namespace DataService.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Title> Titles { get; set; }
}
