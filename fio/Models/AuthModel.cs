using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fio.Models
{
    public class AuthModel
    {
        public bool IsLoggedIn { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public int UserId { get; set; }
    }
}