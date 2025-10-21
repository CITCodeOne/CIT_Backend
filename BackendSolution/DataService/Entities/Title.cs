using DataService.Util;

namespace DataService.Entities;

public class Title : Page
{
    public string Id { get; set; }
    public string Name { get; set; }
    public enum MediaType { } // Placeholder for MediaType enum
    public double AvgRating { get; set; }
    public int NumVotes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool Adult { get; set; }
    public Year4 StartYear { get; set; }
    public Year4 EndYear { get; set; }
    public TimeSpan Runtime { get; set; }
    public string Poster { get; set; }
    public string PlotPre { get; set; } // Shortened to x amount of cars. full plot is then gotten via a separate call 
    public List<Enum> Genres { get; set; } // Or whatever type Genre is
}
