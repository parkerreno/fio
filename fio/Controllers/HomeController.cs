using fio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fio.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var model = new AuthModel();
            if (IsAuthorized(model))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        public ActionResult Login()
        {
            var model = new AuthModel();
            if (IsAuthorized(model))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        public ActionResult Blog()
        {
            var model = new AuthModel();
            IsAuthorized(model);
            return View(model);
        }

        public ActionResult Contact()
        {
            var model = new AuthModel();
            IsAuthorized(model);
            return View(model);
        }

        public ActionResult About()
        {
            var model = new AuthModel();
            IsAuthorized(model);
            return View(model);
        }

        private bool IsAuthorized(AuthModel model)
        {
            if (Session["UserId"] == null)
            {
                return false;
            }

            using (var db = new SqlLinkDataContext())
            {
                model.UserId = (int)Session["UserId"];
                model.Username = (string)Session["Username"];
                model.Name = (string)Session["UserRealname"];
                model.IsLoggedIn = true;

                return true;
            }
        }
    }
}