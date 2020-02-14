using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    public class AssemblyLightOrder
    {
        
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string MaterialId { get; set; }
        [Required]
        public int Status { get; set; }

    }
}
