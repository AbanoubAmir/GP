using OpinionMining.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            /*Task T = Task.Run(() => pf.GetNews(Query));
            Task NT = T.ContinueWith(t => Console.WriteLine("In ContinueWith"));
            NT.Wait();*/
            pf.FetchTweets(Query);
            return View(pf.scores);
        }
    }
}
