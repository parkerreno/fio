using fio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fio.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            var model = new AuthModel();
            if (!IsAuthorized(model))
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View(model);
        }

        /// <summary>
        /// Checks if the current session is an authorised user
        /// </summary>
        /// <param name="model">The model, must inherit from <see cref="AuthModel"/> </param>
        /// <returns>True if authorized, false if not logged in</returns>
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