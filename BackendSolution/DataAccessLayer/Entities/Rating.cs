using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Rating
{
    public int Uconst { get; set; }

    public string Tconst { get; set; } = null!;

    public int? Rating1 { get; set; }

    public DateTime? Time { get; set; }

    public virtual Title TconstNavigation { get; set; } = null!;

    public virtual UserInfo UconstNavigation { get; set; } = null!;
}
