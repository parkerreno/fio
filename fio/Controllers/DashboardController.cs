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

        public ActionResult Portfolio(int id)
        {
            PortfolioModel model = new PortfolioModel();
            if (!IsAuthorized(model))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new SqlLinkDataContext())
            {
                if (db.Fios.Any(f => f.Id == id))
                {
                    var portfolio = db.Fios.Single(f => f.Id == id);
                    if (model.UserId != portfolio.UserId)
                    {
                        return new HttpStatusCodeResult(403, "That portfolio doesn't belong to you!");
                    }
                    model.Portfolio = portfolio;
                    model.Bills = portfolio.Bills.ToArray();
                    model.People = portfolio.Payers.ToArray();

                    return View(model);
                }
                else
                {
                    return new HttpNotFoundResult("That ID doesn't exist :(");
                }
            }
        }

        public ActionResult Bill(int id)
        {
            var model = new BillModel();
            if (!IsAuthorized(model))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new SqlLinkDataContext())
            {
                if (db.Bills.Any(b => b.Id == id))
                {
                    var bill = db.Bills.Single(f => f.Id == id);
                    if (model.UserId != bill.Fio.UserId)
                    {
                        return new HttpStatusCodeResult(403, "That bill doesn't belong to you!");
                    }
                    model.Bill = bill;
                    model.Payers = bill.PaymentDetails.Select(pd => new KeyValuePair<Payer, PaymentDetail>(pd.Payer, pd)).ToArray();

                    return View(model);
                }
                else
                {
                    return new HttpNotFoundResult("That ID doesn't exist :(");
                }
            }
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

        public static string PayLink(int pdId)
        {
            using (var db = new SqlLinkDataContext())
            {
                var pd = db.PaymentDetails.Single(p => p.Id == pdId);

                return $"https://venmo.com/{pd.Bill.Fio.User.VenmoId}?txn=pay&amount={(double)pd.Bill.RAmount * pd.RPercent}";
            }
        }

        public static string RequestLink(int pdId)
        {
            using (var db = new SqlLinkDataContext())
            {
                var pd = db.PaymentDetails.Single(p => p.Id == pdId);

                return $"https://venmo.com/{pd.Payer.VenmoId}?txn=charge&amount={(double)pd.Bill.RAmount * pd.RPercent}";
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string AddOrdinal(int num)
        {
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }
}