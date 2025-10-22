using DataService.Data;

namespace DataService.Entities;

public abstract class Page
{
    public int id { get; set; }
}

public class PageService
{
    public PageService(CITContext ctx)
    {
    }
}
