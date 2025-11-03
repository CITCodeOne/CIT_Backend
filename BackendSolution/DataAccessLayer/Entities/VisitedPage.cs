using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class VisitedPage
{
    public int Uconst { get; set; }

    public int? Pconst { get; set; }

    public DateTime Time { get; set; }

    public required virtual Page PconstNavigation { get; set; }

    public virtual UserInfo UconstNavigation { get; set; } = null!;
}
