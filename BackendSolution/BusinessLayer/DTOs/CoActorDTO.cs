namespace BusinessLayer.DTOs;

// Result of the mdb.find_co_actors database function
public class CoActorDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long CollaborationCount { get; set; }
}
