using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fio.Models;

namespace fio.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Create a user account
        /// </summary>
        /// <param name="username">Desired username <see cref="User.Username"/> </param>
        /// <param name="venmoId">Venmo Identifier for the user to receive payments <see cref="User.VenmoId"/> </param>
        /// <param name="password">Password <see cref="User.Password"/> </param>
        /// <returns>Redirects to dashboard if user created, goes back to signup if error</returns>
        [HttpPost]
        public ActionResult CreateAccount(string username, string venmoId, string password)
        {
            using (var db = new SqlLinkDataContext())
            {
                if (db.Users.Any(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return RedirectToAction("Index", new { Error = "Username taken" });
                }

                var user = new User()
                {
                    Username = username,
                    Password = password,
                    VenmoId = venmoId
                };

                db.Users.InsertOnSubmit(user);
                db.SubmitChanges();

                return RedirectToAction("Login", new { username = user.Username, password = user.Password});
            }
        }

        /// <summary>
        /// Check if a username is available
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>OK (200) if available, CONFLICT (409) if unavailable</returns>
        [HttpPost]
        public ActionResult CheckUsername(string username)
        {
            using (var db = new SqlLinkDataContext())
            {
                if (db.Users.Any(u=>u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new HttpStatusCodeResult(409);
                }
                else
                {
                    return new HttpStatusCodeResult(200);
                }
            }
        }

        public ActionResult Login(string username, string password)
        {
            using (var db = new SqlLinkDataContext())
            {
                if (db.Users.Any(u=>u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) && u.Password.Equals(password)))
                {
                    var user = db.Users.Single(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                    Session["UserId"] = user.Id;
                    Session["Username"] = user.Username;
                    Session["UserRealname"] = user.RealName;

                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOut()
        {
            Session["UserId"] = null;
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}