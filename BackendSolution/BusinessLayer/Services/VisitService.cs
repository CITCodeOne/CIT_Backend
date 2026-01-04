using AutoMapper;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

// Service til at registrere og hente hvilke sider brugere har besøgt.
// Forklaring for ikke-teknikere: hver gang en bruger åbner en side gemmes et "visit",
// så vi bagefter kan vise en historik eller anbefalinger baseret på besøg.
public class VisitService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Konstruktør: modtager database-kontekst og mapper
    public VisitService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapper til DTO'er
    }

    // Hent besøg (visits) for en bruger sorteret med nyeste først
    public List<VisitedPageDTO> GetVisitsByUserId(int userId)
    {
        var visits = _ctx.VisitedPages
            .Where(v => v.Uconst == userId)
            .OrderByDescending(v => v.Time)
            .ToList();

        return _mapper.Map<List<VisitedPageDTO>>(visits);
    }

    // Tilføj et nyt besøg for en bruger på en given side
    // Returnerer DTO for det oprettede visit
    public VisitedPageDTO AddVisitedPage(int userId, int pageId)
    {
        // Opret objekter men tjek først at siden findes
        var visitedPage = new VisitedPage
        {
            Uconst = userId,
            Pconst = pageId,
            Time = DateTime.UtcNow,
            PconstNavigation = null! // rely on existing page FK;
        };

        var pageExists = _ctx.Pages.Any(p => p.Pconst == pageId);
        if (!pageExists) throw new ArgumentException($"Page with id '{pageId}' does not exist.");

        _ctx.VisitedPages.Add(visitedPage);
        _ctx.SaveChanges();

        return _mapper.Map<VisitedPageDTO>(visitedPage);
    }
}
