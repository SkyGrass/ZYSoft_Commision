using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.Comparison
{
    /// <summary>
    /// 
    /// </summary>
    public class ComparisonCreateViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 图标名称
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
    }
}
