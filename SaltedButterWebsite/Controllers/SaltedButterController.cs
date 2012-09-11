using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Mvc;
using SaltedButterWebsite.Models;
using SaltedButterWebsite.ViewModel;

namespace SaltedButterWebsite.Controllers
{
    public class SaltedButterController : Controller
    {
        PlaceSatedButterDataContext _dataContext = new PlaceSatedButterDataContext();

        //
        // GET: /SaltedButter/       
        public JsonResult Index()
        {           
            var placeSaltedButterList = from action in _dataContext.Actions
                                        where action.CategoryId == 1
                                        select action;            
            return Json(placeSaltedButterList, JsonRequestBehavior.AllowGet);                                                                                            
        }

        //
        // GET: /SaltedButter/ID
        public JsonResult ID(int id)
        {            
            var placeSaltedButterList = from action in _dataContext.Actions
                                        where action.CategoryId == 1 && action.ID == id
                                        select action;

            return Json(placeSaltedButterList, JsonRequestBehavior.AllowGet); 
        }

        public ActionResult Note(int id)
        {
            var saltedButterPlace = (from action in _dataContext.Actions
                                        where action.CategoryId == 1 && action.ID == id
                                        select action).Single();

            var saltedButterViewModel = new SaltedButterViewModel()
            {
                Plate = saltedButterPlace.SaltedButter.Plate,
                Address = new string[] { saltedButterPlace.Place.Address1, saltedButterPlace.Place.Address2, saltedButterPlace.Place.PostalCode + " " + saltedButterPlace.Place.City },
                Author = saltedButterPlace.User.Username,
                Comment = saltedButterPlace.SaltedButter.Comment,
                Interval = GetInterval(saltedButterPlace.CreationDate),
                Name = saltedButterPlace.Place.Name
            };

            return PartialView(saltedButterViewModel);
        }

        public ActionResult NoteAdd(string id)
        {                      
            string _address = "http://demo.places.nlp.nokia.com/places/v1/places/" + id + "?app_id=EOXbMEWYAllPhQnAQsmn&app_code=9TIppnJDB9PHy8-ckJLWXA";
            SaltedButterViewModel place = new SaltedButterViewModel(); 
            HttpClient client = new HttpClient();            
            client.DefaultRequestHeaders.Add("Accept", "application/json");           
            
            // Send a request asynchronously continue when complete
            var taskGet = client.GetAsync(_address).ContinueWith(
            (requestTask) =>
            {
                // Get HTTP response from completed task. 
                HttpResponseMessage response = requestTask.Result; 
 
                // Check that response was successful or throw exception 
                response.EnsureSuccessStatusCode();

                response.Content.ReadAsAsync(typeof(NokiaPlace)).ContinueWith(
                (readTask) =>
                {
                    NokiaPlace nokiaPlace = (NokiaPlace)readTask.Result;                    
                    place.AddressText = nokiaPlace.Location.Address["text"];
                    place.PlaceId = nokiaPlace.PlaceId;
                         
                });
                   
            });

            taskGet.Wait();
            return PartialView(place);                                                                   
        }

        //
        // POST: /SaltedButter/Create

        [HttpPost]
        public ActionResult Create(SaltedButterViewModel place)
        {
            string placeName;

            if (ModelState.IsValid)
            {
                placeName = place.Name;
                return Json(new { url = "Map" });
            }
            return PartialView("NoteAdd",place);
            
        }

        private string GetInterval(DateTime date)
        {
            TimeSpan d = DateTime.Now - date;
            string returnValue = string.Empty;
            if (d.Days > 0)
            {

                decimal month = Math.Round((decimal)(d.Days / 30));
                if (month > 12)
                {
                    decimal year = Math.Round((decimal)(d.Days / 365));
                    returnValue = year.ToString() + " an(s)";
                }
                else if (month > 0)
                {
                    returnValue = month.ToString() + " mois";
                }
                else
                {
                    returnValue = d.Days.ToString() + " jour(s)";
                }
            }
            else if (d.Hours > 0)
            {
                returnValue = d.Hours.ToString() + " heure(s)";
            }
            else if (d.Minutes > 0)
            {
                returnValue = d.Minutes.ToString() + " minute(s)";
            }
            else                 
            {
                returnValue = " moins d'une minute";
            }

            return returnValue;
        }
    }
}
