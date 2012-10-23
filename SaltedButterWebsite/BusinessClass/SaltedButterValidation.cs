using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using SaltedButterWebsite.Models;
using SaltedButterWebsite.ViewModel;

namespace SaltedButterWebsite.BusinessClass
{
    public class SaltedButterValidation : ValidationAttribute
    {
         PlaceSatedButterDataContext _dataContext = new PlaceSatedButterDataContext();

         public double Latitude { get; set; }
         public double Longitude { get; set; }

         public override bool IsValid(object value)
         {
             string placeName = (string)value;
             int existenceIndice = PlaceExistence(placeName, this.Latitude, this.Longitude);
             if (existenceIndice == 0)
             {
                 return true;
             }
             else
             {
                 return false;
             }
         }       

        private int PlaceExistence(string name, double latitude, double longitude)
        {
            double[] rangeLat = { latitude - 0.01, longitude + 0.01 };

            var placeList = from place in _dataContext.Places
                            where place.Name == name
                            || (place.Latitude > rangeLat[0] && place.Longitude < rangeLat[1])
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
    }
}