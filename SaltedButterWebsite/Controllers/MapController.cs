using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SaltedButterWebsite.ViewModel;
using SaltedButterWebsite.BusinessClass;

namespace SaltedButterWebsite.Controllers
{
    public class MapController : Controller
    {
        //
        // GET: /Map/
        public ActionResult Index(MapViewModel map)
        {
            if (map == null)
            {
                map = new MapViewModel();
            }
            return View(map);
        }

        public JsonResult SearchPlace(string searchString)
        {
            var url = "http://demo.places.nlp.nokia.com/places/v1/discover/search?at=48.856,2.352&q=REQUEST&size=10&app_id=EOXbMEWYAllPhQnAQsmn&app_code=9TIppnJDB9PHy8-ckJLWXA";

            var search = searchString.Trim();

            //PlaceSearch.GetPlace(search);

            return Json(null, JsonRequestBehavior.AllowGet);
        }
        
    }
}
