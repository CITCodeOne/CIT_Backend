using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

public class PageService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Constructor with dependency injection
    public PageService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    // Get page by ID
    public PageReferenceDTO? GetPageById(int pageId)
    {
        var page = _ctx.Pages.FirstOrDefault(p => p.Pconst == pageId);
        return page == null ? null : _mapper.Map<PageReferenceDTO>(page);
    }

    // Get page by title ID
    public PageReferenceDTO? GetPageByTitleId(string tconst)
    {
        var page = _ctx.Pages.FirstOrDefault(p => p.Tconst == tconst);
        return page == null ? null : _mapper.Map<PageReferenceDTO>(page);
    }
}
