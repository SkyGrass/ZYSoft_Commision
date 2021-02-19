using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commission.Api.Entities
{
    /// <summary>
    /// 用户与业务员关系表
    /// </summary>
    public class UserSalesmanViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
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
    }
}
