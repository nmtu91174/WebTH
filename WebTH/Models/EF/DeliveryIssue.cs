using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTH.Models.EF
{
    [Table("tb_DeliveryIssue")]
    public class DeliveryIssue
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string IssueType { get; set; }
        public string Description { get; set; }
        public bool IsFinal { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Order Order { get; set; }
    }
}