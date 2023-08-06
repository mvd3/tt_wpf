using System;
using System.Collections.Generic;

namespace TeleTrader.Database;

public partial class Exchange
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Symbol> Symbols { get; set; } = new List<Symbol>();
}
