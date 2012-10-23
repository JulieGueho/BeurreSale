using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SaltedButterWebsite.Models;
using SaltedButterWebsite.BusinessClass;

namespace SaltedButterWebsite.ViewModel
{
    
    public class SaltedButterViewModel
    {
        #region Private properties
        //RegEx validating email format. Src :http://msdn.microsoft.com/en-us/library/01escwtf.aspx
        private const string validEmailFormat = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
        #endregion

        #region Public properties   

        #region Validation properties
        // Name of the place
        // Can be defined by the creator if the place to create is an address instead of a defined place such as a restaurant
        [Required(ErrorMessage = "Vous devez entrer un nom de lieu.")]
        [Remote("PlaceExist","SaltedButter",ErrorMessage="Celui-là on l'a déjà.", AdditionalFields="Latitude,Longitude")]
        public string Name { get; set; }

        // Does this place offers salted butter
        [Required(ErrorMessage = "Vous devez indiquer si on peut trouver du beurre salé à cette adresse ou non.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Vous devez entrer votre nom.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vous devez entrer votre adresse email.")]
        [RegularExpression(validEmailFormat, ErrorMessage = "Format incorrect.")]
        public string Email { get; set; }

        #endregion

        public string Plate { get; set; }
        public string Comment { get; set; }

        [AllowHtml]
        public string AddressText { get; set; }
        public string PlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Period since the place was created
        public string Interval { get; set; }

        #endregion

        #region Construction

        // Empty Constructor
        public SaltedButterViewModel()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets address to French address format
        /// </summary>
        /// <param name="place">Place from database</param>
        /// <returns>String French formatted</returns>
        public string GetFrenchAddressFormat(Models.Place place)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(place.Address1))
            {
                builder.Append(place.Address1 + "<br/>");
            }
            
            if (!string.IsNullOrEmpty(place.Address2))
            {
                builder.Append(place.Address2 + "<br/>");
            }
            
            builder.Append(place.PostalCode + " " + place.City);

            return builder.ToString();
        }

        /// <summary>
        /// Sets address to French address format
        /// </summary>
        /// <param name="place">Place from Nokia API</param>
        /// <returns>string of the formatted address</returns>
        public string GetFrenchAddressFormat(Models.NokiaPlace place)
        {           
            return place.Location.Address["text"];
        }

        #endregion

    }
}