using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class RestaurantPhoto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; }

        [StringLength(500)]
        public string Caption { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
        public bool IsCover { get; set; } // Có phải ảnh bìa không?

        [Required]
        public int RestaurantId { get; set; }
        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; }
    }
}
