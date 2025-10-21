namespace DataService.Entities;

public class Contribution
{
    public Individual individual { get; set; }
    public Title title { get; set; }
    public string Type { get; set; }
    public string? Detail { get; set; }
    // the relation on the db also has a priority. We can add it later if needed. But it might only be useful when ordering contributions when they are returned as a list, which can be handled by the SQL query and thus by the DB.
}
