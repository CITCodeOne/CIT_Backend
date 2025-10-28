using System.ComponentModel.DataAnnotations.Schema;

namespace DataService.Entities;
public class Genre
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    //public virtual ICollection<Title>? Titles { get; set; }
    public List<Title>? Titles { get; set; }
}