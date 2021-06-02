using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Tests.Controllers
{
    [Controller]
    public class Home : Controller
    {


        public Home(Application App)
        {

        }

        [Route("")]
        [Route("{*url:regex(^(?!assets).*$)}")] /* redirect to index.html. */
        public IActionResult Index()
        {
            string Main = Path.Combine(TestApp.WebRoot, "index.html");

            //if (!Request.Headers.TryGetValue("X-Requested-With", out var RequestedWith))
                return PhysicalFile(Main, "text/html; charset=UTF-8");

            //string Target = Path.Combine(TestApp.WebRoot, "assets",
            //    string.Format("{0}.html", Request.Path.Value.Trim('/')));

            ///* Ajax Page Requests. */
            //if (RequestedWith.Count <= 0 || !System.IO.File.Exists(Target))
            //    return NotFound();

            //return PhysicalFile(Target, "text/html; charset=UTF-8");
        }


    }
}
