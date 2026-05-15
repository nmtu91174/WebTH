using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models
{
    [Table("tb_VetClinic")]
    public class VetClinic
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên phòng khám không được để trống")]
        [StringLength(250)]
        public string ClinicName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(250)]
        public string Website { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(500)]
        public string Logo { get; set; }

        public string Description { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedDate { get; set; }

        public string VerifiedBy { get; set; }

        public int? YearsInBusiness { get; set; }

        // JSON: ["Chó", "Mèo", "Chim"]
        [StringLength(500)]
        public string SpecializationAreas { get; set; }

        public int TotalRecommendations { get; set; } = 0;

        public decimal AverageRating { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string ModifiedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<VetDoctor> Doctors { get; set; }

        public virtual ICollection<VetRecommendation> Recommendations { get; set; }

        public virtual ICollection<VetClinicRating> Ratings { get; set; }
    }
}
