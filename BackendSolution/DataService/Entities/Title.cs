using DataService.Util;

namespace DataService.Entities;

public class Title : Page
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public MediaType MediaType { get; set; }
    public double AvgRating { get; set; }
    public int NumVotes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool Adult { get; set; }
    public Year4? StartYear { get; set; }
    public Year4? EndYear { get; set; }
    public TimeSpan Runtime { get; set; }
    public string? Poster { get; set; }
    public string? PlotPre { get; set; } // Shortened to x amount of cars. full plot is then gotten via a separate call 
    public List<Genre>? Genres { get; set; }
    public List<Rating>? Ratings { get; set; }
}
