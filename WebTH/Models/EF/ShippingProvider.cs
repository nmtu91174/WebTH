using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models.EF
{
    [Table("tb_ShippingProvider")]
    public class ShippingProvider
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
    }
}