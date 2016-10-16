using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fio.Models
{
    public class CreatePortfolio
    {
        public string Name { get; set; }

        public Payer[] Roommates { get; set; }
    }
}