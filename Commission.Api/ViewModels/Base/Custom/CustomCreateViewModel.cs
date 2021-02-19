using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.Custom
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomCreateViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LinkName{ get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LinkPhone { get; set; }
        public string CardNo { get; set; }
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
