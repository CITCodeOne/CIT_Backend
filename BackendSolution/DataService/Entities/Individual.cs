using DataService.Util;

namespace DataService.Entities;

public class Individual : Page
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Year4 BirthYear { get; set; }
    public Year4 DeathYear { get; set; }
}
