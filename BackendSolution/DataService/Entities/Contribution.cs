namespace DataService.Entities;

public class Contribution
{
    // Foreign key properties for composite primary key
    public required string IndividualId { get; set; } // maps to pconst
    public required string TitleId { get; set; } //maps to tconst
    public string? ContributionType { get; set; } // Maps to priority in BD

    // Navigation properties
    public Individual? individual { get; set; }
    public Title? title { get; set; }

    // Additional property
    public string? Detail { get; set; }
   
}
