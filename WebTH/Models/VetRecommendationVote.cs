using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTH.Models
{
    [Table("tb_VetRecommendationVote")]
    public class VetRecommendationVote
    {
        [Key]
        public int Id { get; set; }

        public int RecommendationId { get; set; }

        public string UserId { get; set; }

        [StringLength(150)]
        public string UserName { get; set; }

        public bool IsHelpful { get; set; }  // true: Hữu ích, false: Không hữu ích

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("RecommendationId")]
        public virtual VetRecommendation VetRecommendation { get; set; }
    }
}
