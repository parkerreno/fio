﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fio.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(string username, string venmoId, string password)
        {
            throw new NotImplementedException();
        }
    }
}