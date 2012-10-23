using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SaltedButterWebsite.Models;

namespace SaltedButterWebsite.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {           
            return View();
        }

        public ActionResult Create()
        {
            TestModel test = new TestModel();
            return View(test);
        }

        [HttpPost]
        public ActionResult Create(TestModel test)
        {
            if (ModelState.IsValid)
            {
                string Name = test.Name;
                string Email = test.Email;

                return Redirect("/");
            }
            else
            {
                return View(test);
            }
        }

    }
}
