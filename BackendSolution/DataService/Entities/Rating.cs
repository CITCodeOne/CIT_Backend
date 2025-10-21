namespace DataService.Entities;

public class Rating : UserAction
{
    public Title Title { get; set; }
    public int RatingValue { get; set; }
}
