using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.Entities
{
    public class DST_DistributeBatchTaskItemDto
    {
        public string userCallCodePath { get; set; }
        public string podCode { get; set; }
        public string podDir { get; set; }
        public string priority { get; set; }
        public string robotCode { get; set; }
        public string taskCode { get; set; }
    }
}
