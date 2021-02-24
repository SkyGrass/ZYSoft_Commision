using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.Entities
{
    public class SaveForm
    {
        public T_Bill Form;

        public List<T_BillEntry> Entry;
    }
}
