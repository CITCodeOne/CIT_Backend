using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

// Service til at hente sider (page) fra databasen.
public class PageService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Konstruktør: får database-konteksten og mapperen injiceret
    public PageService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database-kontekst
        _mapper = mapper; // Mapper til at konvertere entiteter til DTO'er
    }

    // Hent en side ud fra dens interne id (pconst)
    // Returnerer null hvis siden ikke findes
    public PageReferenceDTO? GetPageById(int pageId)
    {
        var page = _ctx.Pages.FirstOrDefault(p => p.Pconst == pageId);
        return page == null ? null : _mapper.Map<PageReferenceDTO>(page);
    }

    // Hent en side ud fra titel-id (tconst)
    public PageReferenceDTO? GetPageByTitleId(string tconst)
    {
        var page = _ctx.Pages.FirstOrDefault(p => p.Tconst == tconst);
        return page == null ? null : _mapper.Map<PageReferenceDTO>(page);
    }
}
