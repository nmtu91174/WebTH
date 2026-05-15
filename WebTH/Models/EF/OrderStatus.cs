using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models.EF
{
    [Table("tb_OrderStatus")]
    public class OrderStatus
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
    }
}