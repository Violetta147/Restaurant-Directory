using Microsoft.AspNetCore.Identity;

namespace PBL3.Models
{
    public enum GenderType // Enum để định nghĩa các lựa chọn giới tính
    {
        Male,
        Female,
        Other
    }

    public class AppUser : IdentityUser
    {
        public string? DisplayName { get; set; } // Tên hiển thị (có thể khác UserName)
        public virtual ICollection<Address> Addresses { get; set; } // Sổ địa chỉ của người dùng
        public GenderType? Gender { get; set; } // Sử dụng enum cho giới tính
        public DateTime? DateOfBirth { get; set; } // Ngày sinh, cho phép null

        // public bool IsProfileCompleted { get; set; } // Cờ bạn có thể dùng
        public virtual ICollection<UserPromotionUsage> PromotionUsages { get; set; }
        public AppUser()
        {
            Addresses = new HashSet<Address>();
            PromotionUsages = new HashSet<UserPromotionUsage>();

        }
    }
}