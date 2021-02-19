using Commission.Api.Entities;
using Commission.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.Utils
{
    public class IDHelper
    {
        private readonly DncZeusDbContext _dbContext;
        public IDHelper(DncZeusDbContext context)
        {
            _dbContext = context;
        }
        public int GenId(string tableName)
        {
            int Id = 0;
            try
            {
                DataTable dt = _dbContext.Database.SqlQuery(string.Format(@"EXEC dbo.L_P_GetMaxID  '{0}',1", tableName));
                if (dt != null && dt.Rows.Count > 0)
                {
                    Id = int.Parse(dt.Rows[0]["FMaxID"].ToString());
                }
            }
            catch (Exception)
            {
                Id = -1;
            }
            return Id;
        }
    }
}
