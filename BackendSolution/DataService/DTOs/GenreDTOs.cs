using DataService.Entities;

namespace DataService.DTOs;

// using record makes it easier to empliment more in the future
public record GenreFullDTO
(
  int Id,
  string Name,
  List<Title> Titles
);

public record GenreReferenceDTO
(
  int Id,
  string Name
);