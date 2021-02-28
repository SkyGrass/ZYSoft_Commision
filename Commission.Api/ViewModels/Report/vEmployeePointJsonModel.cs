using Commission.Api.Entities;
using System;
using System.Collections.Generic;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Report
{
    /// <summary>
    /// 
    /// </summary>
    public class vEmployeePointJsonModel
    {
        public int FSalesmanId { get; set; }
        public int FCustomId { get; set; }
        public string FSalesmanName { get; set; }
        public string FCustomCode { get; set; }
        public string FCustomName { get; set; }
        public string FSoftwareCode { get; set; }
        public string FSoftwareName { get; set; }
        public decimal FContractPrice { get; set; }
        public decimal FStandardPrice { get; set; }
        public decimal FCommissionPrice { get; set; }
        public DateTime FDate { get; set; }
    }
}
