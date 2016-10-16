using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fio.Models
{
    public class BillModel : AuthModel
    {
        public Bill Bill { get; set; }

        public KeyValuePair<Payer, PaymentDetail>[] Payers { get; set; }
    }
}