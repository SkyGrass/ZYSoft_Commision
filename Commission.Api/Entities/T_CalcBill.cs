using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Entities
{
    [Table(name: "T_CalcBill")]
    public class T_CalcBill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FId { get; set; }
        public string FBillNo { get; set; }
        public DateTime FDate { get; set; }
        public int FSalesmanId { get; set; }
        public int? FBillerId { get; set; }
        public bool FIsDeleted { get; set; }
        public AuditState FStatus { get; set; }
        public string FRemark { get; set; }
        public DateTime? FCreatedOn { get; set; }

    }
}
