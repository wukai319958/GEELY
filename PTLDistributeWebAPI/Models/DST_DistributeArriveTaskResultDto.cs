using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    [DataContract]
    public class DST_DistributeArriveTaskResultDto
    {
        [DataMember]
        public string code { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string reqCode { get; set; }

        [DataMember]
        public string data { get; set; }
    }
}