using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models.EF
{
    [Table("tb_ShipmentHistory")]
    public class ShipmentHistory
    {
        [Key] public int Id { get; set; }
        public int ShipmentId { get; set; }
        public int StatusId { get; set; }
        public string Note { get; set; }
        public string Location { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }

        public virtual Shipment Shipment { get; set; }
        public virtual OrderStatus Status { get; set; }
    }
}