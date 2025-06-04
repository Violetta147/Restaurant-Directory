using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PBL3.Models
{
    public class ReviewPhoto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; }

        [StringLength(500)]
        public string Caption { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int ReviewId { get; set; }
        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; }
    }
}
