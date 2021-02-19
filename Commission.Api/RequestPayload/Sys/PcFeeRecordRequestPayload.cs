using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class PcFeeRecordRequestPayload : RequestPayload
    {
        public string KeyWord { get; set; }
        public string FBeginDate { get; set; }
        public string FEndDate { get; set; }
        public int FHospitalID { get; set; }
        //public int FPersonID { get; set; }
    }
}
