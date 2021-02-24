using Commission.Api.Entities;
using System;
using System.Collections.Generic;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordJsonModel
    {
        public int FId { get; set; }
        public int FEntryId { get; set; }
        public string FBillNo { get; set; }
        public DateTime FDate { get; set; }  
        public int FSalesmanId { get; set; }
        public string FSalesmanName { get; set; }
        public int FCustomId { get; set; }
        public string FCustomName { get; set; }
        public int FConfirmerId { get; set; }
        public string FConfirmerName { get; set; }
        public int FSoftwareId { get; set; }
        public string FSoftwareName { get; set; }
        public string FRemark { get; set; } 
        public string FModule { get; set; }
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FDcRate { get; set; }
        public int FPoints { get; set; }
        public bool? FIsCommission { get; set; }
        public AuditState FStatus { get; set; }
        public bool FIsDeleted { get; set; }
    }
}
