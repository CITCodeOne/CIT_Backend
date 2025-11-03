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
    
    // Contributor mappings
    CreateMap<Contributor, ContributorFullDTO>()
      .ForMember(dto => dto.Tconst, opt => opt.MapFrom(c => c.Tconst))
      .ForMember(dto => dto.Iconst, opt => opt.MapFrom(c => c.Iconst))
      .ForMember(dto => dto.Contribution, opt => opt.MapFrom(c => c.Contribution))
      .ForMember(dto => dto.Detail, opt => opt.MapFrom(c => c.Detail))
      .ForMember(dto => dto.Priority, opt => opt.MapFrom(c => c.Priority))
      .ForMember(dto => dto.Title, opt => opt.MapFrom(c => c.TconstNavigation != null ? c.TconstNavigation : null));

    CreateMap<Contributor, ContributorPreviewDTO>()
      .ForMember(dto => dto.Tconst, opt => opt.MapFrom(c => c.Tconst))
      .ForMember(dto => dto.Iconst, opt => opt.MapFrom(c => c.Iconst))
      .ForMember(dto => dto.Contribution, opt => opt.MapFrom(c => c.Contribution))
      .ForMember(dto => dto.Priority, opt => opt.MapFrom(c => c.Priority));

    CreateMap<Contributor, ContributorReferenceDTO>()
      .ForMember(dto => dto.Tconst, opt => opt.MapFrom(c => c.Tconst))
      .ForMember(dto => dto.Iconst, opt => opt.MapFrom(c => c.Iconst));

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

    // VisitedPage mappings
    CreateMap<VisitedPage, VisitedPageDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(vp => vp.Uconst))
      .ForMember(dto => dto.PageId, opt => opt.MapFrom(vp => vp.Pconst))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(vp => vp.Time));

    CreateMap<VisitedPage, VisitedPageWithDetailsDTO>()
      .ForMember(dto => dto.UserId, opt => opt.MapFrom(vp => vp.Uconst))
      .ForMember(dto => dto.PageId, opt => opt.MapFrom(vp => vp.Pconst))
      .ForMember(dto => dto.Time, opt => opt.MapFrom(vp => vp.Time))
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(vp => vp.PconstNavigation.Tconst))
      .ForMember(dto => dto.IndividualId, opt => opt.MapFrom(vp => vp.PconstNavigation.Iconst));
      
    // Wi mappings
    CreateMap<Wi, WiDTO>()
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(w => w.Tconst))
      .ForMember(dto => dto.Word, opt => opt.MapFrom(w => w.Word))
      .ForMember(dto => dto.Field, opt => opt.MapFrom(w => w.Field.ToString()))
      .ForMember(dto => dto.Lexeme, opt => opt.MapFrom(w => w.Lexeme));

    // ActorTitleView mappings
    CreateMap<ActorTitleView, ActorTitleViewDTO>()
      .ForMember(dto => dto.TitleId, opt => opt.MapFrom(a => a.Tconst ?? string.Empty))
      .ForMember(dto => dto.TitleName, opt => opt.MapFrom(a => a.TitleName ?? string.Empty))
      .ForMember(dto => dto.IndividualId, opt => opt.MapFrom(a => a.Iconst ?? string.Empty))
      .ForMember(dto => dto.IndividualName, opt => opt.MapFrom(a => a.Name ?? string.Empty))
      .ForMember(dto => dto.Contribution, opt => opt.MapFrom(a => a.Contribution ?? string.Empty));

    // Episode mappings
    CreateMap<Episode, EpisodeDTO>()
      .ForMember(dto => dto.Id, opt => opt.MapFrom(e => e.Tconst))
      .ForMember(dto => dto.ParentId, opt => opt.MapFrom(e => e.Parenttconst))
      .ForMember(dto => dto.Season, opt => opt.MapFrom(e => e.Snum))
      .ForMember(dto => dto.EpisodeNumber, opt => opt.MapFrom(e => e.Epnum));
  }
}
