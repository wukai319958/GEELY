using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    public class Assembling
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string AreaId { get; set; }
        [Required]
        [MaxLength(64)]
        public string XGateIP { get; set; }
        [Required]
        public int Bus { get; set; }
        [Required]
        public int Address { get; set; }

        [Required]
        [MaxLength(32)]
        public string Type { get; set; }

        [Required]
        [MaxLength(64)]
        public string MaterialId { get; set; }
        [Required]
        public int Status { get; set; }

    }
}
