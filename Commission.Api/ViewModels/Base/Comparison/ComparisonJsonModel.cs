using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.Comparison
{
    /// <summary>
    /// 
    /// </summary>
    public class ComparisonJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SoftwareId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CommisionWayId { get; set; }

        public decimal StartPeriod { get; set; }
        public decimal EndPeriod { get; set; }
        public decimal Proportion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CreatedByUserGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedByUserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ModifiedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ModifiedByUserGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ModifiedByUserName { get; set; }
    }
}
