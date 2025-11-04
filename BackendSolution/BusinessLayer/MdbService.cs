using DataAccessLayer.Data;
using BusinessLayer.Services;
using AutoMapper;

namespace BusinessLayer;

public class MdbService // Potentially needs to implement some interface : IDataService
{
    // DataService setup
    readonly CITContext _ctx;
    readonly IMapper _mapper;

    // For dependency injection we only use a constructor with parameters.
    public MdbService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    // Services as properties with lazy initialization (i.e., only initialized when first accessed)
    private TitleService? _titleService;
    public TitleService Title => _titleService ??= new TitleService(_ctx, _mapper);

    private IndividualService? _individualService;
    public IndividualService Individual => _individualService ??= new IndividualService(_ctx, _mapper);

    private RatingService? _ratingService;
    public RatingService Rating => _ratingService ??= new RatingService(_ctx, _mapper);
}
