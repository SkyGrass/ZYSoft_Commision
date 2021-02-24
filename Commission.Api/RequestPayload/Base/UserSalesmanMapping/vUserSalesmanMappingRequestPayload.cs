using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Base.UserSalesmanMapping
{
    /// <summary>
    /// 请求参数实体
    /// </summary>
    public class vUserSalesmanMappingRequestPayload : RequestPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SalesmanName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SalesmanCode { get; set; }
    }
}
