using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Bookmark
{
    public int Uconst { get; set; }

    public int Pconst { get; set; }

    public DateTime? Time { get; set; }

    public virtual Page PconstNavigation { get; set; } = null!;

    public virtual UserInfo UconstNavigation { get; set; } = null!;
}
