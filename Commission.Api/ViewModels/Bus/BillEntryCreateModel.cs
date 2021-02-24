using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.ViewModels.Bus
{
    public class BillEntryCreateModel
    {
        public int FId { get; set; }
        public int FSoftwareId { get; set; }
        public string FModule { get; set; } 
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FDcRate { get; set; }
        public int FPoints { get; set; }
    }
}
