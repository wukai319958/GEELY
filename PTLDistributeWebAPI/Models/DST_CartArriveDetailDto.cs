using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace PTLDistributeWebAPI.Models
{
    [DataContract]
    public class DST_CartArriveDetailDto
    {
        [DataMember]
        public string currentPosition { get; set; }

        [DataMember]
        public string podCode { get; set; }
    }
}