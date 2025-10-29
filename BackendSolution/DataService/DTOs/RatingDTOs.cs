namespace DataService.DTOs;

// Used to return a user's rating
public class RatingDTO
{
  public int UserId { get; set; }
  public string TitleId { get; set; } = string.Empty;
  public int Rating { get; set; }
  public DateTime? Time { get; set; }
}

// Used to show a user's rating with title details
public class RatingWithTitleDTO
{
  public int UserId { get; set; }
  public string TitleId { get; set; } = string.Empty;
  public int Rating { get; set; }
  public DateTime? Time { get; set; }
  public TitleReferenceDTO? Title { get; set; }
}

// Used for showing ratings for a title with user details
public class RatingWithUserDTO
{
  public int UserId { get; set; }
  public string UserName { get; set; } = string.Empty;
  public string TitleId { get; set; } = string.Empty;
  public int Rating { get; set; }
  public DateTime? Time { get; set; }
}

// to create a new rating
public class CreateRatingDTO
{
  public string TitleId { get; set; } = string.Empty;
  public int Rating { get; set; }
}
