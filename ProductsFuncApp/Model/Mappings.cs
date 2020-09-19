using System;
using System.Collections.Generic;
using System.Text;

namespace ProductsFuncApp.Model
{
    public static class Mappings
    {
        public static ProductEntity ToTableEntity(this ProductModel product)
        {
            return new ProductEntity()
            {
                PartitionKey = "PRODUCT",
                RowKey = product.Id,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CreatedTime = product.CreatedTime,
            };
        }

        public static ProductModel ToProductModel(this ProductEntity product)
        {
            return new ProductModel()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CreatedTime = product.CreatedTime,
            };
        }
    }
}
