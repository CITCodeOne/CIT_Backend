using AutoMapper;
using DataService.Entities;
using DataService.DTOs;

namespace DataService.Mappings;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    // Title mappings
    CreateMap<Title, TitleFullDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(t => t.Tconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(t => t.TitleName))
      .ForMember(dto => dto.MediaType, opt => opt.MapFrom(t => t.MediaType ?? ""))
      .ForMember(dto => dto.AvgRating, opt => opt.MapFrom(t => t.AvgRating ?? 0))
      .ForMember(dto => dto.NumVotes, opt => opt.MapFrom(t => t.Numvotes ?? 0))
      .ForMember(dto => dto.ReleaseDate, opt => opt.MapFrom(t => t.ReleaseDate.HasValue ? t.ReleaseDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue))
      .ForMember(dto => dto.Adult, opt => opt.MapFrom(t => t.IsAdult ?? false))
      .ForMember(dto => dto.StartYear, opt => opt.MapFrom(t => t.StartYear ?? 0))
      .ForMember(dto => dto.EndYear, opt => opt.MapFrom(t => t.EndYear.HasValue ? (int?)t.EndYear.Value : null))
      .ForMember(dto => dto.Runtime, opt => opt.MapFrom(t => t.Runtime.HasValue ? (int)t.Runtime.Value.TotalMinutes : 0))
      .ForMember(dto => dto.Poster, opt => opt.MapFrom(t => t.Poster))
      .ForMember(dto => dto.PlotPre, opt => opt.MapFrom(t => t.Plot != null && t.Plot.Length > 25 ? t.Plot.Substring(0, 25) : t.Plot))
      .ForMember(dto => dto.Genres, opt => opt.MapFrom(t => t.Gconsts));

    CreateMap<Title, TitlePreviewDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(t => t.Tconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(t => t.TitleName ?? ""))
      .ForMember(dto => dto.MediaType, opt => opt.MapFrom(t => t.MediaType ?? ""))
      .ForMember(dto => dto.AvgRating, opt => opt.MapFrom(t => t.AvgRating ?? 0))
      .ForMember(dto => dto.ReleaseDate, opt => opt.MapFrom(t => t.ReleaseDate.HasValue ? t.ReleaseDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue))
      .ForMember(dto => dto.Poster, opt => opt.MapFrom(t => t.Poster ?? ""));

    CreateMap<Title, TitleReferenceDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(t => t.Tconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(t => t.TitleName ?? ""));

    // Genre mappings
    CreateMap<Genre, GenreDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(g => g.Gconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(g => g.Gname ?? ""));

    CreateMap<Genre, GenreFullDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(g => g.Gconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(g => g.Gname ?? ""))
      .ForMember(dto => dto.Titles, opt => opt.MapFrom(g => g.Tconsts));

    // Rating mappings
    CreateMap<Rating, RatingDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(r => r.Uconst))
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(r => r.Tconst))
      .ForMember(dto => dto.Rating, opt => opt.MapFrom(r => r.Rating1 ?? 0))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(r => r.Time));

    CreateMap<Rating, RatingWithTitleDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(r => r.Uconst))
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(r => r.Tconst))
      .ForMember(dto => dto.Rating, opt => opt.MapFrom(r => r.Rating1 ?? 0))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(r => r.Time))
      .ForMember(dto => dto.Title, opt => opt.MapFrom(r => r.TconstNavigation));

    CreateMap<Rating, RatingWithUserDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(r => r.Uconst))
      .ForMember(dto => dto.UserName, opt => opt.MapFrom(r => r.UconstNavigation.UserName ?? ""))
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(r => r.Tconst))
      .ForMember(dto => dto.Rating, opt => opt.MapFrom(r => r.Rating1 ?? 0))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(r => r.Time));
      
    // Individual mappings
    CreateMap<Individual, IndividualFullDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(i => i.Iconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(i => i.Name))
      .ForMember(dto => dto.BirthYear, opt => opt.MapFrom(i => i.BirthYear))
      .ForMember(dto => dto.DeathYear, opt => opt.MapFrom(i => i.DeathYear))
      .ForMember(dto => dto.NameRating, opt => opt.MapFrom(i => i.NameRating));

    CreateMap<Individual, IndividualReferenceDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(i => i.Iconst))
      .ForMember(dto => dto.Name, opt => opt.MapFrom(i => i.Name ?? ""));

      // Bookmark mappings
    CreateMap<Bookmark, BookmarkDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(b => b.Uconst))
      .ForMember(dto => dto.PageId, opt => opt.MapFrom(b => b.Pconst))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(b => b.Time));

    CreateMap<Bookmark, BookmarkWithDetailsDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(b => b.Uconst))
      .ForMember(dto => dto.PageId, opt => opt.MapFrom(b => b.Pconst))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(b => b.Time))
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(b => b.PconstNavigation.Tconst))
      .ForMember(dto => dto.IndividualId, opt => opt.MapFrom(b => b.PconstNavigation.Iconst));
  }
}
