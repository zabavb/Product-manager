using Azure;
using Azure.Data.Tables;
using System;

namespace Exam_first_task_csh
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }

        public string ImgUrl { get; set; }
        public string ImgBlobId { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public Product() { }

        public Product(string id, string name, double price, string description, string imgUrl)
        {
            PartitionKey = "Product";
            RowKey = id;
            Name = name;
            Price = price;
            Description = description;
            ImgUrl = imgUrl;
        }
    }
}