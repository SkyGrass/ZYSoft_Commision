using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.RequestPayload.Base.CommisionWay
{
    /// <summary>
    /// 请求参数实体
    /// </summary>
    public class CommisionWayRequestPayload : RequestPayload
    {
        /// <summary>45
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }
    }
}
