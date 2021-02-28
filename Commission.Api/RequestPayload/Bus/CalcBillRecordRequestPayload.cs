using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class CalcBillRecordRequestPayload : RequestPayload
    {
        public string KeyWord { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public int SalesmanId { get; set; }
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
