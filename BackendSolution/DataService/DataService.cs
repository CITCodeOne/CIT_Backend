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

    public PageService Page => new PageService(_ctx);

}
