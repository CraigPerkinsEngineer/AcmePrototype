using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcmePrototype.Models
{
    public class Product
    {
        public string CustomerId { get; set; }

        public string ProductName { get; set; }

        public string Domain { get; set; }

        public string DurationMonths { get; set; }

        public DateTime StartDate { get; set; }
    }
}
