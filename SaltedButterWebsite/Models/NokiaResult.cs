using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaltedButterWebsite.Models
{
    public class NokiaResult
    {
        public Result Results { get; set; }
    }

    public class Result
    {
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public double[] Position { get; set; }
        public int Distance { get; set; }
        public string Title { get; set; }
        public double AverageRating { get; set; }
        public Category Category { get; set; }
        public string Icon { get; set; }
        public string Vicinity { get; set; }
        public string Id { get; set; }

    }

    public class Category
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Href { get; set; }
        public string Type { get; set; }
    }
}