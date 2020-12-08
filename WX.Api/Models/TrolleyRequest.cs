using System.Collections.Generic;

namespace WX.Api.Models
{
    public class TrolleyRequest
    {
        public List<Product> Products { get; set; }
        public List<Special> Specials { get; set; }
        public List<Product> Quantities { get; set; }
    }
}