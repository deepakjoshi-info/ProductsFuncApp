using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ProductsFuncApp
{
    public class ProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
    public static class Product
    {
        static List<ProductModel> products = new List<ProductModel>();
        static Product()
        {
            products.Add(new ProductModel()
            {
                Id = "1",
                Name = "Mobile",
                Price = 100m,
                Stock = 10
            });
            products.Add(new ProductModel()
            {
                Id = "2",
                Name = "HDD",
                Price = 100m,
                Stock = 10
            });
            products.Add(new ProductModel()
            {
                Id = "3",
                Name = "TVs",
                Price = 100m,
                Stock = 10
            });
        }

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> GetProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Product")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting product list.");
            return new OkObjectResult(products);
        }

        [FunctionName("GetProductbyId")]
        public static async Task<IActionResult> GetProductbyId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Product/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("Getting product by id");
            var product = products.FirstOrDefault(t => t.Id == id);
            if (product == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(product);
        }
        
        [FunctionName("AddProduct")]
        public static async Task<IActionResult> AddProduct(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Product")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Adding a new product in list");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonConvert.DeserializeObject<ProductModel>(requestBody);
            products.Add(product);
            return new OkObjectResult(product);
        }
        
        [FunctionName("DeleteProduct")]
        public static IActionResult DeleteProduct(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Product/{id}")] HttpRequest req,
    ILogger log, string id)
        {
            var product = products.FirstOrDefault(t => t.Id == id);
            if (product == null)
            {
                return new NotFoundResult();
            }
            products.Remove(product);
            return new OkResult();
        }
    }
}
