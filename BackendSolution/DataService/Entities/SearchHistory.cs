using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class SearchHistory
{
    public int Uconst { get; set; }

    public DateTime Time { get; set; }

    public string? SearchString { get; set; }

    public virtual UserInfo UconstNavigation { get; set; } = null!;
}
