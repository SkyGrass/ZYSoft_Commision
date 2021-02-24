using Commission.Api.Entities;
using System;
using System.Collections.Generic;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordEntryJsonModel
    {
        public int FEntryId { get; set; }
        public int FId { get; set; }
        public int FSoftwareId { get; set; }
        public string FModule { get; set; }
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FDcRate { get; set; }
        public int FPoints { get; set; }
    }
}
