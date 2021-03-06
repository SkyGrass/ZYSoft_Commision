﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.Entities
{
    public class T_BillEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FEntryID { get; set; }
        public int FId { get; set; }
        public int FSoftwareId { get; set; }
        public string FModule { get; set; }
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FDcRate { get; set; }
        public int FPoints { get; set; } 
    }
}
