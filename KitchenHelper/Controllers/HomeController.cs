using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KitchenHelper.Models;

namespace KitchenHelper.Controllers
{
    public class HomeController : Controller
    {
        private KitchenHelperDB db = new KitchenHelperDB();

        //
        // GET: /Home/
        public ActionResult Index()
        {
            var recipes = from r in db.Recipes orderby r.LastViewed descending select r;
            ViewBag.Top10 = recipes.Take(10).ToList();
            return View();
        }

    }
}
