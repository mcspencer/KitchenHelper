using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KitchenHelper.Models
{
    public class Recipe
    {
        public int ID { get; set; }
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Ingredients { get; set; }

        [DataType(DataType.MultilineText)]
        public string Method { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [DataType(DataType.Time)]
        public string EstimatedPrepTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateAdded { get; set; }

        public DateTime? LastViewed { get; set; }
    }
}