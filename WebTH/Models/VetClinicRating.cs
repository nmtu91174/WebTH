using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models
{
    [Table("tb_VetClinicRating")]
    public class VetClinicRating
    {
        [Key]
        public int Id { get; set; }

        public int ClinicId { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(150)]
        public string UserName { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Đánh giá không được để trống")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime? VisitDate { get; set; }

        [Range(1, 5)]
        public int? ServiceQuality { get; set; }  // 1-5

        [Range(1, 5)]
        public int? Professionalism { get; set; }  // 1-5

        [Range(1, 5)]
        public int? Facility { get; set; }  // 1-5

        [Range(1, 5)]
        public int? PriceValue { get; set; }  // 1-5

        public bool IsVerified { get; set; } = false;

        public bool IsApproved { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string ModifiedBy { get; set; }

        // Navigation Property
        [ForeignKey("ClinicId")]
        public virtual VetClinic VetClinic { get; set; }
    }
}
