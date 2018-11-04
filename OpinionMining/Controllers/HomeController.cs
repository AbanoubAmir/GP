using OpinionMining.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace OpinionMining.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string userName = WebConfigurationManager.AppSettings["url"];
            List<Tuple<string, float?>> scores = new List<Tuple<string, float?>>();
            scores.Add(new Tuple<string, float?>("", 0));
            return View(scores);
        }

        [HttpPost]
        public ActionResult Index(string Query)
        {
            PrototypeFunctions pf = new PrototypeFunctions();
            return View(pf.FetchTweets(Query));
        }
    }
}
