using System;
using System.Collections.Generic;
using ystem.Linq;
using System.Web;
using System.Web.Mvc;

namespace SayNoToSQL.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            //trivialComment
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}