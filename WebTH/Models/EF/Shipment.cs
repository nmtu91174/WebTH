using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models.EF
{
    [Table("tb_Shipment")]
    public class Shipment
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProviderId { get; set; }
        public string TrackingCode { get; set; }
        public int StatusId { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public decimal ShippingFee { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Order Order { get; set; }
        public virtual ShippingProvider Provider { get; set; }
        public virtual OrderStatus Status { get; set; }
        public virtual ICollection<ShipmentHistory> Histories { get; set; }
            = new HashSet<ShipmentHistory>();
    }
}