using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    [Table("Restaurants")]
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        // Thêm các thuộc tính mở rộng sau này
        // public bool IsVerified { get; set; }
        // public string OwnerId { get; set; }
    }
}
