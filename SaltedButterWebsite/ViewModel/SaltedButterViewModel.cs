using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SaltedButterWebsite.ViewModel
{
    public class SaltedButterViewModel
    {        
        public string[] Address { get; set; }
        public string AddressText { get; set; }       
        public string PlaceId { get; set; }        

        [Required(ErrorMessage = "Le nom du lieu est obligatoire")]
        public string Name { get; set; }
        public string Plate { get; set; }
        public string Comment { get; set; }
        public string Author { get; set; }
        public string Interval { get; set; }
        
    }
}