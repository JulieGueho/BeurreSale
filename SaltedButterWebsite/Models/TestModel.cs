using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SaltedButterWebsite.Models
{

    public class TestModel
    {
        [Required(ErrorMessage="Ce champ est obligatoire")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ce champ est obligatoire")]
        public string Email { get; set; }
    }
}