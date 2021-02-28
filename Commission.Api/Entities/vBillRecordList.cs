using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Entities
{
    public class vBillRecordList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FId { get; set; } 
        public string FBillNo { get; set; }
        public DateTime FDate { get; set; }
        public int FSalesmanId { get; set; }
        public int FCustomId { get; set; }
        public int? FBillerId { get; set; }
        public int? FConfirmerId { get; set; }
        public bool FIsDeleted { get; set; }
        public bool FIsCommission { get; set; }
        public AuditState FStatus { get; set; }
        public string FRemark { get; set; }
        public DateTime? FCreatedOn { get; set; }
        public string FCustomName { get; set; }
        public string FConfirmerName { get; set; }
        public string FSalesmanName { get; set; }
        public bool FIsCalc { get; set; }

    }
}
