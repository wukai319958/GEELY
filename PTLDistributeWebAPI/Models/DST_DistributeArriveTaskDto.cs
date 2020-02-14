using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    [DataContract]
    public class DST_DistributeArriveTaskDto
    {
        [DataMember]
        public List<DST_CartArriveDetailDto> data { get; set; }

        [DataMember]
        public string method { get; set; }

        [DataMember]
        public string taskType { get; set; }

        [DataMember]
        public string reqCode { get; set; }

        [DataMember]
        public DateTime reqTime { get; set; }

        [DataMember]
        public string robotCode { get; set; }

        [DataMember]
        public string taskCode { get; set; }
    }
}