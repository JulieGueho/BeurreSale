using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Linq;
using System.Web;
using SaltedButterWebsite.Models;

namespace SaltedButterWebsite.BusinessClass
{
    public class PlaceSearch
    {
        public PlaceSearch(Models.Place place)
        {
            this.DbId = place.ID;
            this.Name = place.Name;
            this.Latitude = place.Latitude;
            this.Longitude = place.Longitude;
            this.AlreadyInDb = true;
            this.IsPrecisePlace = true;
        }

        public PlaceSearch(Item place)
        {
            this.NokiaId = place.Id;
            this.Name = place.Title;
            this.Latitude = place.Position[0];
            this.Longitude = place.Position[1];
            this.AlreadyInDb = false;
        }

        public string NokiaId { get; set; }

        public int DbId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool AlreadyInDb { get; set; }

        public bool IsPrecisePlace { get; set; }

        public bool Center{get;set;}

        public static Item[] ApiSearch(string initialSearchString)
        {
            string url = "http://demo.places.nlp.nokia.com/places/v1/discover/search?at=48.856,2.352&q=REQUEST&size=10&app_id=EOXbMEWYAllPhQnAQsmn&app_code=9TIppnJDB9PHy8-ckJLWXA";

            //Cleaning search 
            var searchString = initialSearchString.Trim();

            // Replacing parameters in the request
            var urlrequest = url.Replace("REQUEST", searchString);

            HttpClient client = new HttpClient();            
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            NokiaResult result = null;

            // Send a request asynchronously continue when complete
            var taskGet = client.GetAsync(url).ContinueWith(
            (requestTask) =>
            {
                // Get HTTP response from completed task. 
                HttpResponseMessage response = requestTask.Result; 
 
                // Check that response was successful or throw exception 
                response.EnsureSuccessStatusCode();

                response.Content.ReadAsAsync(typeof(NokiaResult)).ContinueWith(
                (readTask) =>
                {
                    result = (NokiaResult)readTask.Result;
                    
                });
            });

            taskGet.Wait();

            //Filter Nokia results
            return result.Results.Items;
        }

        public static IQueryable<Models.Place> DbSearch(string initialSearchString)
        {
            var cleanString = initialSearchString.Trim().Replace(',', ' ').Replace(';', ' ');
            string[] words = cleanString.Split(' ');
            IQueryable<Models.Place> places = null;

            using (PlaceSatedButterDataContext context = new PlaceSatedButterDataContext())
            {
                places = context.Places.Where(p => p.Name == words[0]);
                if (words.Count() > 1)
                {
                    var filteredPlaces = places.Where(p => p.City == words[1]);
                    if (filteredPlaces == null)
                    {
                        places = filteredPlaces;
                    }
                }

            }

            return places;
        }

        public static void GetList(string initialSearchString)
        {
            Item[] nokiaPlaces = ApiSearch(initialSearchString);
            IQueryable<Models.Place> dbPlaces = DbSearch(initialSearchString);

            List<PlaceSearch> list = new List<PlaceSearch>();
            foreach (Item place in nokiaPlaces)
            {
                var dbPlaceWithSameName = dbPlaces.Where(p => p.Name == place.Title);
                foreach (var dbPlace in dbPlaceWithSameName)
                {
                    list.Add(new PlaceSearch(dbPlace));
                    if (!IsTheSamePlace(place.Position, new double[] { dbPlace.Latitude, dbPlace.Longitude }))
                    {
                        list.Add(new PlaceSearch(place));
                    }
                }

                if(dbPlaceWithSameName == null)
                {
                    list.Add(new PlaceSearch(place));
                }
            }
        }

        private static bool IsTheSamePlace(double[] position1, double[] position2)
        {

            double distance = Math.Acos(Math.Sin(DegreeToRadian(position1[0])) * Math.Sin(DegreeToRadian(position2[0])) + Math.Cos(DegreeToRadian(position1[0])) * Math.Cos(DegreeToRadian(position2[0])) * Math.Cos(DegreeToRadian(position1[1] - position2[1]))) * 6371;

            if (distance > 0.5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private static bool SortNokiaPlace()
        {
            return false;
        }


    }
}