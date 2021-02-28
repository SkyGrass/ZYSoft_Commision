using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.RequestPayload.Report
{
    public class EmployeeRequestPayload : RequestPayload
    {
        public int SalesmanId { get; set; }
        public int CustomId { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
    }
}
