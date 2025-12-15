using AutoMapper;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

public class VisitService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Constructor with dependency injection
    public VisitService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    public List<VisitedPageDTO> GetVisitsByUserId(int userId)
    {
        var visits = _ctx.VisitedPages
            .Where(v => v.Uconst == userId)
            .OrderByDescending(v => v.Time)
            .ToList();

        return _mapper.Map<List<VisitedPageDTO>>(visits);
    }

    public VisitedPageDTO AddVisitedPage(int userId, int pageId)
    {
        // Create new visited page object
        var visitedPage = new VisitedPage
        {
            Uconst = userId,
            Pconst = pageId,
            Time = DateTime.UtcNow,
            PconstNavigation = new Page { Pconst = pageId } // needed as PconstNavigation is non-nullable
        };

        // validate that the page exists
        var pageExists = _ctx.Pages.Any(p => p.Pconst == pageId);
        if (!pageExists) throw new ArgumentException($"Page with id '{pageId}' does not exist.");

        _ctx.VisitedPages.Add(visitedPage);
        _ctx.SaveChanges();

        return _mapper.Map<VisitedPageDTO>(visitedPage);
    }
}
