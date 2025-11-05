namespace BusinessLayer.DTOs;

// Flattened DTO for actor-title view rows
public class ActorTitleViewDTO
{
    public string TitleId { get; set; } = string.Empty;
    public string TitleName { get; set; } = string.Empty;
    public string IndividualId { get; set; } = string.Empty;
    public string IndividualName { get; set; } = string.Empty;
    public string Contribution { get; set; } = string.Empty;
}

