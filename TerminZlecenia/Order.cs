using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminZlecenia
{
    public class Order
    {
        public string OrderName { get; set; }
        public int Quantity { get; set; }
        public DateTime? EstimatedEndTime { get; set; }

        public Order(string orderName, int quantity, DateTime estimatedEndtime)
        {
            this.OrderName = orderName;
            this.Quantity = quantity;
            this.EstimatedEndTime = estimatedEndtime;
        }
    }
}
