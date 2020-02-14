using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributing.Entities
{
    public class MarketZoneDto
    {
        public string AreaId { get; set; }
        public int CFG_CartId { get; set; }
        public int Position { get; set; }
        public string Direction { get; set; }
        public string BatchCode { get; set; }
    }
}
