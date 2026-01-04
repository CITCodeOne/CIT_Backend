/*
    MdbService.cs - overblik (dansk)

    Kort forklaring:
    - Denne klasse fungerer som en enkel 'facade' over flere forretningsservices
        (Title, Individual, User, Bookmark, Rating, Auth, Visit, Page). Den samler
        afhængigheder (database, mapper, hashing) og laver lazy-initialisering af
        de konkrete services.

    CITContext:
    - `CITContext` er Entity Framework Core DbContext'en defineret i
        DataAccessLayer/Data/CITContext.cs. Den holder konfigurationen for
        databaseforbindelsen (læser `dbconfig.json`), DbSet<>-definitioner for
        tabeller/views og model-konfiguration i `OnModelCreating`.

    Mapping / AutoMapper:
    - `MappingProfile` (BusinessLayer/Mappings/MappingProfile.cs) er en AutoMapper
        `Profile` der beskriver hvordan database-entities oversættes til DTO'er.
        Et `IMapper`-objekt injiceres i denne service og bruges af de underliggende
        services til at konvertere mellem entity- og DTO-typer.

    Hashing:
    - `Hashing` (BusinessLayer/Services/HashingService.cs) er en simpel hashing-service
        som genererer et tilfældigt salt og en SHA-256 hash af et password. Metoden
        `Hash(string)` returnerer en tuple `(hash, salt)` som gemmes ved brugeroprettelse,
        og `Verify(password, storedHash, storedSalt)` sammenligner et login-password med
        den gemte hash via samme salt.

    Registrering i opstart:
    - I WebServiceLayer/Program.cs registreres `CITContext` via `AddDbContext<CITContext>`,
        AutoMapper via `AddAutoMapper(typeof(MappingProfile).Assembly)` og `Hashing` som
        singleton (`AddSingleton<Hashing>()`).

*/
using DataAccessLayer.Data;
using BusinessLayer.Services;
using AutoMapper;

namespace BusinessLayer;

//MdbService might be considered a facade for accessing various services
public class MdbService // Potentially needs to implement some interface : IDataService
{
    // DataService setup
    readonly CITContext _ctx;
    readonly IMapper _mapper;
    readonly Hashing _hashing;

    // For dependency injection we only use a constructor with parameters.
    public MdbService(CITContext ctx, IMapper mapper, Hashing hashing)
    {
        _ctx = ctx;
        _mapper = mapper;
        _hashing = hashing;
    }

    // Services as properties with lazy initialization (i.e., only initialized when first accessed)
    private TitleService? _titleService;
    public TitleService Title => _titleService ??= new TitleService(_ctx, _mapper);

    private IndividualService? _individualService;
    public IndividualService Individual => _individualService ??= new IndividualService(_ctx, _mapper);

    private UserService? _userService;
    public UserService User => _userService ??= new UserService(_ctx, _mapper);

    private BookmarkService? _bookmarkService;
    public BookmarkService Bookmark => _bookmarkService ??= new BookmarkService(_ctx, _mapper);

    private RatingService? _ratingService;
    public RatingService Rating => _ratingService ??= new RatingService(_ctx, _mapper);

    private AuthService? _authService;
    public AuthService Auth => _authService ??= new AuthService(_ctx, _hashing);

    private VisitService? _visitService;
    public VisitService Visit => _visitService ??= new VisitService(_ctx, _mapper);

    private PageService? _pageService;
    public PageService Page => _pageService ??= new PageService(_ctx, _mapper);
}
