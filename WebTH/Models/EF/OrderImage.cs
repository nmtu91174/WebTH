using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTH.Models.EF
{
    [Table("tb_OrderImage")]
    public class OrderImage
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Order Order { get; set; }
    }
}