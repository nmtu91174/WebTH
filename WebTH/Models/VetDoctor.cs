using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models
{
    [Table("tb_VetDoctor")]
    public class VetDoctor
    {
        [Key]
        public int Id { get; set; }

        public int ClinicId { get; set; }

        [Required(ErrorMessage = "Tên bác sĩ không được để trống")]
        [StringLength(150)]
        public string DoctorName { get; set; }

        [StringLength(500)]
        public string Qualifications { get; set; }  // VD: "Thạc sĩ Thú Y, 10 năm kinh nghiệm"

        [StringLength(200)]
        public string Specialization { get; set; }  // VD: "Ngoại vết thương", "Dinh dưỡng"

        [StringLength(500)]
        public string Avatar { get; set; }

        public string Bio { get; set; }

        public int? ExperienceYears { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string ModifiedBy { get; set; }

        // Navigation Property
        [ForeignKey("ClinicId")]
        public virtual VetClinic VetClinic { get; set; }
    }
}
