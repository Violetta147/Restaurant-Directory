using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using PBL3.Models;

namespace PBL3.ViewModels 
{
    public class RegisterRestaurantViewModel
    {
        [Required(ErrorMessage = "Tên nhà hàng không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên nhà hàng không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên nhà hàng")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Url(ErrorMessage = "Địa chỉ website không hợp lệ.")]
        [StringLength(200)]
        [Display(Name = "Website (tùy chọn)")]
        public string Website { get; set; }

        [StringLength(200)]
        [Display(Name = "Giờ mở cửa (ví dụ: 08:00 - 22:00)")]
        public string OpeningHours { get; set; }

        //[Display(Name = "Khoảng giá")]
        //public PriceRange? PriceRange { get; set; } // Enum PriceRange đã định nghĩa trong Restaurant.cs

        [Display(Name = "Ảnh đại diện (tùy chọn)")]
        public IFormFile MainImageFile { get; set; } // Để upload file ảnh

        // --- Thông tin Địa chỉ ---
        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống.")]
        [StringLength(300)]
        [Display(Name = "Địa chỉ (Số nhà, tên đường)")]
        public string AddressLine1 { get; set; }

        [Required(ErrorMessage = "Phường/Xã không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Phường/Xã")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Quận/Huyện không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Tỉnh/Thành phố")]
        public string City { get; set; }

        [StringLength(50)]
        [Display(Name = "Quốc gia")]
        public string Country { get; set; } = "Việt Nam";

        [Display(Name = "Vĩ độ (tùy chọn)")]
        public double? Latitude { get; set; }

        [Display(Name = "Kinh độ (tùy chọn)")]
        public double? Longitude { get; set; }
    }
}