﻿using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.CommisionWay
{
    /// <summary>
    /// 
    /// </summary>
    public class CommisionWayCreateViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 图标名称
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public int HospitalId { get; set; }
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
