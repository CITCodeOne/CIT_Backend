using System.ComponentModel.DataAnnotations.Schema;
using DataService.Data;

namespace DataService.Entities;

public abstract class Page
{
   public int Id { get; set; }
}

public class PageService
{
    public PageService(CITContext ctx)
    {
    }
}
