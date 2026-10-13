
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EComWeb.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }

        public int cust_id { get; set; }

        public int product_id { get; set; }

        public int product_quantity { get; set; }
        public int product_size { get; set; }

        public decimal total_price { get; set; }

        public DateTime order_date { get; set; }

        public int order_status { get; set; } // 0: Chờ xác nhận, 1: Đã xác nhận, 2: Đang giao, 3: Hoàn thành, 4: Hủy

        [ForeignKey("cust_id")]
        public Customer customers { get; set; }

        [ForeignKey("product_id")]
        public Product products { get; set; }
    }
}
