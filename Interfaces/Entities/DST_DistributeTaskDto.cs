using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.Entities
{
    public class DST_DistributeTaskDto
    {
        public string reqCode { get; set; }
        public string reqTime { get; set; }
        public string clientCode { get; set; }
        public string tokenCode { get; set; }
        public string taskTyp { get; set; }
        public string userCallCode { get; set; }
        public List<string> userCallCodePath { get; set; }
        public string podCode { get; set; }
        //public string podDir { get; set; }
        //public string priority { get; set; }
        public string robotCode { get; set; }
        public string taskCode { get; set; }
        public string data { get; set; }
    }
}
