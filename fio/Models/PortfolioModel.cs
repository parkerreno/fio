using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fio.Models
{
    public class PortfolioModel : AuthModel
    {
        public Fio Portfolio { get; set; }

        public BillModel[] Bills { get; set; }

        public Payer[] People { get; set; }
    }
}