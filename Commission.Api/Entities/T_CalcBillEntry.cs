using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.Entities
{
    public class T_CalcBillEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FEntryID { get; set; }
        public int FId { get; set; }
        public int FRecordId { get; set; }
        public int FRecordEntryId { get; set; }
        public decimal FCommissionPrice { get; set; }
        public decimal FExpand { get; set; }
        public decimal FTotal { get; set; }
    }
}
