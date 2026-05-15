using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTH.Models.EF
{
    [Table("tb_Order")]
    public class Order : CommonAbstract
    {
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            this.Shipments = new HashSet<Shipment>(); // Thêm dòng này
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không để trống")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không để trống")]
        public string Address { get; set; }

        public string Email { get; set; }


        public decimal TotalAmount { get; set; }

        public int Quantity { get; set; }

        public int TypePayment { get; set; }

        public int Status { get; set; }

        public string CustomerId { get; set; }


        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        public virtual ICollection<Shipment> Shipments { get; set; }

        // Thêm vào class Order hiện tại của bạn
        public string ShipperId { get; set; }
        public string AgentId { get; set; }
        public bool IsLocked { get; set; }
        public int? PreviousStatus { get; set; }
        public string PickupProof { get; set; }
        public string DeliveryProof { get; set; }
        public string FailedReason { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }

        public virtual ICollection<OrderHistory> OrderHistories { get; set; }
    }
}