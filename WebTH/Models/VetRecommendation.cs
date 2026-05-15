using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTH.Models.EF;

namespace WebTH.Models
{
    [Table("tb_VetRecommendation")]
    public class VetRecommendation
    {
        [Key]
        public int Id { get; set; }

        public int ClinicId { get; set; }

        [ForeignKey("VetDoctor")]
        public int? DoctorId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Loại thú cưng không được để trống")]
        [StringLength(50)]
        public string PetType { get; set; }  // "Chó", "Mèo", "Cả hai"

        [Required(ErrorMessage = "Lý do khuyến cáo không được để trống")]
        public string ReccommendationReason { get; set; }

        [StringLength(200)]
        public string HealthCondition { get; set; }  // "Théo phổi", "Da nhạy cảm", "Tiêu chảy"

        [StringLength(100)]
        public string PetAgeRange { get; set; }  // "0-3 tháng", "3-6 tháng", "Trưởng thành"

        /// <summary>
        /// 1: Khuyến cáo
        /// 2: Khuyến cáo mạnh
        /// 3: Cần thiết
        /// </summary>
        public int RecommendationLevel { get; set; } = 1;

        public bool IsApproved { get; set; } = true;

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public int TotalReferences { get; set; } = 0;  // Số lần được tham khảo

        public int HelpfulCount { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string ModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("ClinicId")]
        public virtual VetClinic VetClinic { get; set; }

        [ForeignKey("DoctorId")]
        public virtual VetDoctor VetDoctor { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
