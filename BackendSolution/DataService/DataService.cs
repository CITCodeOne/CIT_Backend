using Microsoft.EntityFrameworkCore;
using DataService.Entities;
using DataService.Data;

namespace DataService;

public class DataService : IDataService
{
    readonly CITContext? _ctx;

    public DataService()
    {
        _ctx = null;
    }

    public DataService(CITContext ctx)
    {
        _ctx = ctx;
    }

    // EntityServices
}
