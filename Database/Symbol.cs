using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeleTrader.Database;

public partial class Symbol
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Ticker { get; set; } = null!;

    public string? Isin { get; set; }

    public string? CurrencyCode { get; set; }

    public DateTime? DateAdded { get; set; }

    public double? Price { get; set; }

    public DateTime? PriceDate { get; set; }

    public long TypeId { get; set; }

    public long ExchangeId { get; set; }

    public virtual Exchange Exchange { get; set; } = null!;

    public virtual Type Type { get; set; } = null!;
}
