using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTLDistributeWebAPI.Models
{
    public class DST_BindOrUnBindTaskDto
    {
        public string reqCode { get; set; }
        public string reqTime { get; set; }
        public string clientCode { get; set; }
        public string tokenCode { get; set; }
        public string podCode { get; set; }
        public string pointCode { get; set; }
        public string indBind { get; set; }
    }
}
