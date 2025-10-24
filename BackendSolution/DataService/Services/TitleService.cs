using DataService.Data;

namespace DataService.Entities;

public class TitleService
{
    readonly CITContext _context;

    public TitleService(CITContext context)
    {
        _context = context;
    }

    // Methods to interact with Title entities would go here
}
