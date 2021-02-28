using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commission.Api.Entities
{
    public class SaveBillForm
    {
        public T_Bill Form;

        public List<T_BillEntry> Entry;
    }

    public class SaveCalcBillForm
    {
        public T_CalcBill Form;

        public List<T_CalcBillEntry> Entry;
    }
}
