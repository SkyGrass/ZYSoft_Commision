using Commission.Api.Models.Response;

namespace Commission.Api.Extensions
{
    /// <summary>
    /// /
    /// </summary>
    public class ResponseModelFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public static ResponseModel CreateInstance => new ResponseModel();
        /// <summary>
        /// 
        /// </summary>
        public static ResponseResultModel CreateResultInstance => new ResponseResultModel();
    }
}
