using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fio.Models
{
    public class DashboardModel : AuthModel
    {
        public Fio[] Portfolios { get; set; }
    }
}