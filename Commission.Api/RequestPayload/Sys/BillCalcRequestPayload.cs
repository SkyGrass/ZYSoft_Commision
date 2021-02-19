﻿using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillCalcRequestPayload : RequestPayload
    {
        public string KeyWord { get; set; }
        public string FBeginDate { get; set; }
        public string FEndDate { get; set; }
        public int FHospitalID { get; set; }
        public int FAreaID { get; set; }
        public int FBedID { get; set; }
        public int FManagerID { get; set; }
        public string FPBeginDate { get; set; }
        public string FPEndDate { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public IsFinish FIsFinish { get; set; }
    }
}
