﻿using System;
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
            var placeSaltedButterList = _dataContext.Actions.Where(a => a.CategoryId == 1);
                        
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
                UserName = saltedButterPlace.User.Username,
                Comment = saltedButterPlace.SaltedButter.Comment,
                Interval = GetInterval(saltedButterPlace.CreationDate),
                Name = saltedButterPlace.Place.Name,
                Status = saltedButterPlace.SaltedButter.Salted.ToString()
            };

            saltedButterViewModel.AddressText = saltedButterViewModel.GetFrenchAddressFormat(saltedButterPlace.Place);

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
                    // Mapping information provided by API
                    NokiaPlace nokiaPlace = (NokiaPlace)readTask.Result;                    
                    place.AddressText = nokiaPlace.Location.Address["text"];                   
                    place.PlaceId = nokiaPlace.PlaceId;
                    place.Name = nokiaPlace.Name;
                    place.Latitude = nokiaPlace.Location.Position[0];
                    place.Longitude = nokiaPlace.Location.Position[1];
                });
                   
            });

            taskGet.Wait();
            return PartialView(place);                                                                   
        }

        //
        // POST: /SaltedButter/Create

        [HttpPost]
        public ActionResult Create(SaltedButterViewModel submittedPlace)
        {
            if (ModelState.IsValid)
            {
                Models.Place place = new Models.Place();
                string _address = "http://demo.places.nlp.nokia.com/places/v1/places/" + submittedPlace.PlaceId + "?app_id=EOXbMEWYAllPhQnAQsmn&app_code=9TIppnJDB9PHy8-ckJLWXA";                
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
                        // Mapping information provided by API
                        NokiaPlace nokiaPlace = (NokiaPlace)readTask.Result;        
                        place.Address1 = nokiaPlace.Location.Address["street"];
                        place.City = nokiaPlace.Location.Address["city"];
                        place.PostalCode = nokiaPlace.Location.Address["postalCode"];
                        place.Country = nokiaPlace.Location.Address["country"];
                        place.Latitude = nokiaPlace.Location.Position[0];
                        place.Longitude = nokiaPlace.Location.Position[1];
                    });

                });

                place.Name = submittedPlace.Name;

                Models.User user = new Models.User(){
                    Email = submittedPlace.Email,
                    Username = submittedPlace.UserName,
                    CreationDate = DateTime.Now
                };

                Models.SaltedButter saltedButter = new Models.SaltedButter()
                {
                    Salted = Convert.ToBoolean(submittedPlace.Status),
                    Comment = submittedPlace.Comment
                };                

                taskGet.Wait();

                Models.Action action = new Models.Action()
                {
                    Place = place,
                    SaltedButter = saltedButter,
                    User = user,
                    CreationDate = DateTime.Now,
                    StatusId = 1,
                    CategoryId = 1
                };

                _dataContext.Actions.InsertOnSubmit(action);

                _dataContext.SubmitChanges();

                MapViewModel map = new MapViewModel()
                {
                    Latitude = place.Latitude,
                    Longitude = place.Longitude
                };

                //_dataContext.Places.InsertOnSubmit(place);
                TempData["success"] = "ok"; // Indicating to display ok notification after redirection
                return RedirectToAction("Index", "Map", map);
                
            }

            return PartialView("NoteAdd",submittedPlace);
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

        public JsonResult PlaceExist(string name, double latitude, double longitude)
        {
            int existenceIndice = PlaceExistence(name, latitude, longitude);
            if (existenceIndice == 0)
            {
                return Json("Celui-là on l'a déjà.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Cool celui-là on ne l'a pas encore.", JsonRequestBehavior.AllowGet);
            }
        }

        #region Private methods
        /// <summary>
        /// Testing place existence in salted Butter category
        /// </summary>
        /// <param name="name">Name of the place</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>placeId if place exists with no element linked in the specified category, 0 if exists, -1 if place doesn't exist</returns>
        private int PlaceExistence(string name, double latitude, double longitude)
        {            
            double[] rangeLat = {latitude - 0.01, longitude+0.01};

            var placeList = from place in _dataContext.Places
                            where place.Name == name
                            && (place.Latitude > rangeLat[0] && place.Longitude < rangeLat[1])
                            select place;

            if (placeList.Any())
            {
                int placeId = placeList.First().ID;
                bool exist = (from action in _dataContext.Actions
                              where action.CategoryId == 1
                              && action.PlaceId == placeId
                              select action).Any();
                if (exist)
                {
                    return 0;
                }
                else
                {
                    return placeId;
                }
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }
}
