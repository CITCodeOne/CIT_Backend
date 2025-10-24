namespace DataService.DTOs;

// using record makes it easier to empliment more in the future
public record TitleFullDTO
(
  string Id,
  string? Name,
  string MediaType,
  double AvgRating,
  int NumVotes,
  DateTime ReleaseDate,
  bool Adult,
  int StartYear,
  int? EndYear,
  int Runtime,
  string? Poster,
  string? PlotPre

 /* List<Genre> Genres (OPS Are we interested in ratings as well?) */ 
);

public record TitlePreviewDTO
(
  string Id,
  string Name,
  string MediaType,
  double AvgRating,
  DateTime ReleaseDate,
  string Poster
);

public record TitleReferenceDTO
(
  string Id,
  string Name
);