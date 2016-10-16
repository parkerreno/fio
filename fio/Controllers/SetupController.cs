using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fio.Models;
using Newtonsoft.Json;

namespace fio.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup
        public ActionResult Index()
        {
            var model = new AuthModel();
            if (!IsAuthorized(model))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: Register
        public ActionResult Register()
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
        //[HttpPost]
        public ActionResult CreateAccount(string username, string venmoId, string password, string name)
        {
            using (var db = new SqlLinkDataContext())
            {
                if (db.Users.Any(u => u.Username.ToLower() == username.ToLower()))
                {
                    return RedirectToAction("Index", new { Error = "Username taken" });
                }

                var user = new User()
                {
                    Username = username,
                    Password = password,
                    VenmoId = venmoId,
                    RealName = name
                };

                db.Users.InsertOnSubmit(user);
                db.SubmitChanges();

                return RedirectToAction("Login", new { username = user.Username, password = user.Password });
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
                if (db.Users.Any(u => u.Username.ToLower() == username.ToLower()))
                {
                    return new HttpStatusCodeResult(409);
                }
                else
                {
                    return new HttpStatusCodeResult(200);
                }
            }
        }

        [HttpPost]
        public ActionResult CreatePortfolio(string json)
        {
            var model = new AuthModel();
            if (!IsAuthorized(model))
            {
                return new HttpStatusCodeResult(401);
            }
            var data = JsonConvert.DeserializeObject<CreatePortfolio>(json);
            using (var db = new SqlLinkDataContext())
            {
                var portfolio = new Fio()
                {
                    Name = data.Name,
                    UserId = model.UserId
                };
                db.Fios.InsertOnSubmit(portfolio);
                db.SubmitChanges();

                var payers = data.Roommates.Select(r => new Payer() { Name = r.Name, Fio = portfolio, VenmoId = r.VenmoId });
                db.Payers.InsertAllOnSubmit(payers);
                db.SubmitChanges();
                var inc = new IdNameCombo()
                {
                    Id = portfolio.Id,
                    Payers = payers.OrderBy(p => p.Id).Select(p => p.Name).ToArray()
                };
                return Json(inc);
            }

            return new HttpStatusCodeResult(200);
        }

        public ActionResult AddBills(string json, int id)
        {
            var model = new AuthModel();
            if (!IsAuthorized(model))
            {
                return new HttpStatusCodeResult(401);
            }
            json = json.Replace("null", "0");
            var data = JsonConvert.DeserializeObject<Bills>(json);

            using (var db = new SqlLinkDataContext())
            {
                var por = db.Fios.Single(x => x.Id == id);
                var rentBill = new Bill() { Name = "Rent", RAmount = (decimal)data.RentTotal };
                var oneTime = new Bill() { Name = "OneTime", SAmount = (decimal)data.OneTime.Sum() };
                var utilities = new Bill() { Name = "Utilities", RAmount = (decimal)data.Utilities.Sum() };
                por.Bills.Add(rentBill);
                por.Bills.Add(oneTime);
                por.Bills.Add(utilities);
                db.SubmitChanges();
                var pays = por.Payers.OrderBy(p => p.Id);
                int i = 0;
                foreach (var p in pays)
                {
                    try { p.PaymentDetails.Add(new PaymentDetail() { Payer = p, Bill = rentBill, RPercent = data.Rent[i]/100 }); }
                    catch { }
                    try
                    {
                        if (oneTime.SAmount == 0)
                        {
                            oneTime.SAmount = 0.01M;
                        }
                        p.PaymentDetails.Add(new PaymentDetail() { Payer = p, Bill = oneTime, SPercent = data.OneTime[i] / (double)oneTime.SAmount });
                    }
                    catch { }
                    try
                    {
                        if (utilities.RAmount == 0)
                        {
                            utilities.RAmount = 0.01M;
                        }
                        p.PaymentDetails.Add(new PaymentDetail() { Payer = p, Bill = utilities, RPercent = data.Utilities[i] / (double)utilities.RAmount });

                    }
                    catch { }
                    i++;
                }
                try
                {
                    db.SubmitChanges();
                }catch (Exception e)
                {

                }
                
                return new HttpStatusCodeResult(200);
            }
        }

        private class Bills
        {
            public double RentTotal { get; set; }
            public double[] Rent { get; set; }
            public double[] OneTime { get; set; }
            public double[] Utilities { get; set; }
        }

        private class IdNameCombo
        {
            public int Id { get; set; }

            public string[] Payers { get; set; }
        }

        public ActionResult Login(string username, string password)
        {
            using (var db = new SqlLinkDataContext())
            {
                if (db.Users.Any(u => u.Username.ToLower() == username.ToLower() && u.Password.Equals(password)))
                {
                    var user = db.Users.Single(u => u.Username.ToLower() == username.ToLower());
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