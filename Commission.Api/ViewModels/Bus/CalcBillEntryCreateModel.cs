using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.ViewModels.Bus
{
    public class CalcBillEntryCreateModel
    {
        public int FId { get; set; }
        public int FRecordId { get; set; }
        public int FRecordEntryId { get; set; }
        public decimal FCommissionPrice { get; set; }
        public decimal FExpand { get; set; }
        public decimal FTotal { get; set; }
    }
}
