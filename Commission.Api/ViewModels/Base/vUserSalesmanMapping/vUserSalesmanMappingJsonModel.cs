using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commission.Api.ViewModels.Base.vUserSalesmanMapping
{
    /// <summary>
    /// 用户与业务员关系表
    /// </summary>
    public class vUserSalesmanMappingJsonModel
    {
        /// <summary>
        /// 编码
        /// </summary> 
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public Guid UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public int SalesmanId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CreatedByUserGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedByUserName { get; set; }
        public string UserName { get; set; }
        public string SalesmanName { get; set; }
        public string SalesmanCode { get; set; }
    }
}
