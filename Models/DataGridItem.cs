using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleTrader.Models
{
    internal class DataGridItem
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public string Ticker { get; set; }
        public double? Price { get; set; }
        public string ExchangeName { get; set; }
        public string? TypeName { get; set; }
    }
}
