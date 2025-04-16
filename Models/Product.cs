using System.ComponentModel.DataAnnotations;

namespace EComWeb.Models
{
    public class Product
    {
        [Key]
        public int products_id { get; set; }
        public string products_name { get; set; }
        public string products_price { get; set; }
        public string products_description { get; set; }
        public string products_image { get; set; }

        public int cat_id { get; set; }
        public Category Category { get; set; }
    }
}
