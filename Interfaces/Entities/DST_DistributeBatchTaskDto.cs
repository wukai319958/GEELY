using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.Entities
{
    public class DST_DistributeBatchTaskDto
    {
        public string reqCode { get; set; }
        public string reqTime { get; set; }
        public string clientCode { get; set; }
        public string tokenCode { get; set; }
        public string taskTyp { get; set; }
        public string userCallCode { get; set; }
        public string taskGroupCode { get; set; }
        public List<DST_DistributeBatchTaskItemDto> taskGroups { get; set; }
    }
}
