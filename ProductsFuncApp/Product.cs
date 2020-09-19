using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductsFuncApp.Model;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductsFuncApp
{

    public static class Product
    {

        private const string Route = "Product";
        private const string TableName = "products";
        [FunctionName("GetProduct")]
        public static async Task<IActionResult> GetProduct(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
           [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable productTable,
           ILogger log)
        {
            log.LogInformation("Getting product list");
            var query = new TableQuery<ProductEntity>();
            var segment = await productTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToProductModel));
        }

        [FunctionName("GetProductbyId")]
        public static IActionResult GetProductbyId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, "PRODUCT", "{id}", Connection = "AzureWebJobsStorage")] ProductEntity productTable,
            ILogger log, string id)
        {
            log.LogInformation("Getting product by id");
            if (productTable == null)
            {
                log.LogInformation($"Product {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(productTable.ToProductModel());
        }

        [FunctionName("AddProduct")]
        public static async Task<IActionResult> AddProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] IAsyncCollector<ProductEntity> productTable,
            ILogger log)
        {
            log.LogInformation("Adding product in table.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ProductModel>(requestBody);
            var product = Mappings.ToTableEntity(input);
            await productTable.AddAsync(product);
            return new OkObjectResult(product);
        }

       

        [FunctionName("UpdateProduct")]
        public static async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable productTable,
            ILogger log, string id)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ProductModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ProductEntity>("PRODUCT", id);
            var findResult = await productTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (ProductEntity)findResult.Result;
            var replaceOperation = TableOperation.Replace(existingRow);
            await productTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(existingRow.ToProductModel());
        }

        [FunctionName("DeleteProduct")]
        public static async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable productTable,
            ILogger log, string id)
        {
            var deleteEntity = new TableEntity { PartitionKey = "PRODUCT", RowKey = id, ETag = "*" };
            var deleteOperation = TableOperation.Delete(deleteEntity);
            try
            {
                await productTable.ExecuteAsync(deleteOperation);
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }
}
