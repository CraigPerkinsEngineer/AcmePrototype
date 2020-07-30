using AcmePrototype.Models;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AcmePrototype.Controllers
{
    public class HomeController : Controller
    {
        const string sessionKey = "Products";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ListProducts()
        {
            var model = new List<ProductViewModel>();

            var products = HttpContext.Session.GetObjectFromJson<HashSet<ProductViewModel>>(sessionKey);

            if (products != null)
            {
                model = products.OrderBy(p => p.CustomerId).ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var model = new ProductViewModel();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var productProcessed = _processProduct(model);

                if (!productProcessed)
                {
                    ModelState.AddModelError("Products", "Product not processed. Please try again.");

                    return View(model);
                }

                var products = HttpContext.Session.GetObjectFromJson<HashSet<ProductViewModel>>(sessionKey);

                if (products == null)
                {
                    products = new HashSet<ProductViewModel>();
                }

                products.Add(model);

                HttpContext.Session.SetObjectAsJson(sessionKey, products);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        private bool _processProduct(ProductViewModel product)
        {
            switch (product.ProductName)
            {
                case "domain":
                case "edomain":
                    // Bill the customer
                    // Register the domain
                    break;
                case "pdomain":
                    // Bill the customer
                    // Register the domain
                    // Secure the domain
                    break;
                case "hosting":
                    // Bill the customer
                    // Provision the account
                    // Send a welcome email
                    break;
                case "email":
                    // Bill the cutomer
                    // Create email routing
                    break;
            }

            return true;
        }

        private bool ValidateProduct(ProductViewModel product)
        {
            switch (product.ProductName)
            {
                case "domain":
                case "pdomain":
                    if (!product.Domain.EndsWith(".edu"))
                    {
                        ModelState.AddModelError("Domain", "Invalid domain");
                        return false;
                    }
                    // Durations must be positive multiples of 1 year.
                    // Duplicate Domain registrations are not permitted.
                    break;
                case "edomain":
                    if (!product.Domain.EndsWith(".com") && !product.Domain.EndsWith(".org"))
                    {
                        ModelState.AddModelError("Domain", "Invalid domain");
                        return false;
                    }
                    // Durations must be positive multiples of 1 year.
                    // Duplicate Domain registrations are not permitted.
                    break;
                case "hosting":
                    // Durations must be positive multiples of 1 month.
                    // A Domain registration is required.
                    // Duplicate Hosting registrations are not permitted.
                    break;
                case "email":
                    // Durations must be positive multiples of 1 month.
                    // An active Domain registration is required.
                    // Duplicate Email registrations are not permitted.
                    break;
                default:
                    ModelState.AddModelError("ProductName", "Invalid product name");
                    return false;
            }


            return true;
        }

        [HttpGet]
        public IActionResult LoadProducts()
        {
            var model = new LoadProductsViewModel();

            return View(model);
        }

        [HttpPost]
        public IActionResult LoadProducts(LoadProductsViewModel model)
        {
            if (ModelState.IsValid)
            {
                TextReader reader = new StreamReader(model.Products.OpenReadStream());
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csvReader.GetRecords<Product>();

                var products = HttpContext.Session.GetObjectFromJson<HashSet<ProductViewModel>>(sessionKey);

                if (products == null)
                {
                    products = new HashSet<ProductViewModel>();
                }

                var productsProcessed = true;

                var productsFromCsv = records.Select(r => new ProductViewModel()
                {
                    CustomerId = r.CustomerId,
                    Domain = r.Domain,
                    DurationMonths = r.DurationMonths,
                    ProductName = r.ProductName,
                    StartDate = r.StartDate
                });

                foreach(var product in productsFromCsv)
                {
                    productsProcessed = this._processProduct(product) && productsProcessed;

                    products.Add(product);
                }

                if (productsProcessed)
                {
                    HttpContext.Session.SetObjectAsJson(sessionKey, products);
                }
                else
                {
                    ModelState.AddModelError("Products", "Products not processed. Please try again.");

                    return View(model);
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Email()
        {
            var model = new List<ProductViewModel>();

            var products = HttpContext.Session.GetObjectFromJson<HashSet<ProductViewModel>>(sessionKey);

            if (products == null)
            {
                products = new HashSet<ProductViewModel>();
            }

            foreach (var product in products)
            {
                foreach (var emailDate in product.GetEmailDate())
                {
                    model.Add(new ProductViewModel()
                    {
                        CustomerId = product.CustomerId,
                        Domain = product.Domain,
                        DurationMonths = product.DurationMonths,
                        FinalEmailDate = emailDate,
                        ProductName = product.ProductName,
                        StartDate = product.StartDate
                    });
                }
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
