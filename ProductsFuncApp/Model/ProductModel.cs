using System;
using System.Collections.Generic;
using System.Text;

namespace ProductsFuncApp.Model
{
    public class ProductModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
