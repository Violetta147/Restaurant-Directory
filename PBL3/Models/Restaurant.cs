using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace PBL3.Models
{
    [Table("Restaurants")]
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        //vị trí lat long sử dụng NetTopologySuite
        //trong gói Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite
        [Required]
        [Column(TypeName = "geometry")]
        public Point Location { get; set; }

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Range(0, 5)]
        public double Rating { get; set; }

        [StringLength(10)]
        public string PriceLevel { get; set; } = string.Empty; // $ to $$$$

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public bool IsOpen { get; set; } = true;
        
        // Giờ mở cửa và đóng cửa
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }

        [StringLength(500)]
        public string Keywords { get; set; } = string.Empty; // For better search capabilities
        
        // Số lượng đánh giá
        public int ReviewCount { get; set; } = 0;
    }
}
