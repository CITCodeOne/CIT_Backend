using Microsoft.EntityFrameworkCore;
using DataService.Entities;
using DataService.Data;

namespace DataService;

public class DataService : IDataService
{
    // DataService setup

    readonly CITContext _ctx;

    public DataService()
    {
        _ctx = new CITContext();
    }

    public DataService(CITContext ctx)
    {
        _ctx = ctx;
    }

    // EntityServices
    // Add your services here as needed
    // public TitleService Title => new TitleService(_ctx);
}
