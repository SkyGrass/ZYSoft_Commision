using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Entities
{
    public class vCustomPoint
    {
        [Key]
        public int FId { get; set; }
        public int FCustomId { get; set; } 
        public string FCustomCode { get; set; }
        public string FCustomName { get; set; }
        public int FPoints { get; set; }
        public DateTime FDate { get; set; }
    }
}
