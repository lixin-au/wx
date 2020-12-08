using System.Collections.Generic;

namespace WX.Api.Models
{
    public class ShopperHistory
    {
        public long CustomerId { get; set; }
        public List<Product> Products { get; set; }
    }
}