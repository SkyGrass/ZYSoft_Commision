using Commission.Api.Entities;
using System;
using System.Collections.Generic;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Report
{
    /// <summary>
    /// 
    /// </summary>
    public class vCustomPointJsonModel
    {
        public int FId { get; set; }
        public int FCustomId { get; set; }
        public string FCustomCode { get; set; }
        public string FCustomName { get; set; }
        public int FPoints { get; set; }
        public DateTime FDate { get; set; }
    }
}
