using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaltedButterWebsite.Models
{
    public class NokiaPlace
    {
        public string Name { get; set; }
        public string PlaceId { get; set; }
        public Location Location { get; set; }        

        public NokiaPlace() { }
    }

    public class Location
    {
        public decimal[] Position { get; set; }
        public Dictionary<string,string> Address { get; set; }
        //public decimal[] Bbox { get; set; }

        public Location() { }
    }

    public class Address
    {
        string Text { get; set; }
        string House { get; set; }
        string Street { get; set; }
        string District { get; set; }
        string PostalCode { get; set; }
        string City { get; set; }
        string County { get; set; }
        string State { get; set; }
        string Country { get; set; }
        string CountryCode { get; set; }

        public Address() { }
       
    }

    
}