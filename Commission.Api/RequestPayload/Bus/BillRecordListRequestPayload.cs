using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordListRequestPayload : RequestPayload
    {
        public string KeyWord { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public int CustomId { get; set; }
        public int SalesmanId { get; set; }
        public int ConfirmerId { get; set; }
        public YesOrNo IsCommission { get; set; }
        /// <summary> 
        ///  
        /// </summary>
        public AuditState Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
    }
}
