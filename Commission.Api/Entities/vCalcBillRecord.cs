﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Entities
{
    public class vCalcBillRecord
    {
        public int FId { get; set; }
        [Key]
        public int FEntryId { get; set; }
        public string FBillNo { get; set; }
        public DateTime FDate { get; set; }
        public int FSalesmanId { get; set; }
        public int FCustomId { get; set; }
        public int FSoftwareId { get; set; }
        public int? FBillerId { get; set; }
        public bool FIsDeleted { get; set; }
        public AuditState FStatus { get; set; }
        public string FRemark { get; set; }
        public DateTime? FCreatedOn { get; set; }
        public string FSalesmanName { get; set; }
        public string FCustomName { get; set; }
        public string FSoftwareName { get; set; }
        public string FModule { get; set; }
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FCommissionPrice { get; set; }
        public decimal FDcRate { get; set; }
        public int FPoints { get; set; }
        public int FRecordId { get; set; }
        public int FRecordEntryId { get; set; }
        public decimal FExpand { get; set; }
        public decimal FTotal { get; set; }
    }
}
