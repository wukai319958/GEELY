using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    public class CacheRegionLightOrder
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string AreaId { get; set; }
        [Required]
        [MaxLength(64)]
        public string MaterialId  { get; set; }
    }
}
