using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AcmePrototype.Models
{
    public class ProductViewModel
    {
        [Display(Name = "Customer Id")]
        public string CustomerId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Domain")]
        public string Domain { get; set; }

        [Display(Name = "Duration Months")]
        public string DurationMonths { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        public ProductViewModel()
        {
            this.FinalEmailDate = DateTime.Today;
        }

        public HashSet<DateTime> GetEmailDate()
        {
            var dates = new HashSet<DateTime>();
            var expiration = StartDate.AddMonths(int.Parse(DurationMonths));

            switch (ProductName)
            {
                case "domain":
                case "pdomain":
                case "edomain":
                    dates.Add(expiration.AddDays(-2));
                    break;
                case "hosting":
                    dates.Add(StartDate.AddDays(1));
                    dates.Add(expiration.AddDays(-3));
                    break;
                case "email":
                    dates.Add(expiration.AddDays(-2));
                    break;
            }

            return dates;
        }

        public DateTime FinalEmailDate { get; set; }
    }
}