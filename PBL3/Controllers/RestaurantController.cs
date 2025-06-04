//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
//using Microsoft.EntityFrameworkCore; // Cần cho Include và các thao tác DB
//using PBL3.Data; // Namespace của ApplicationDbContext
//using PBL3.Models; // Namespace của các Models (Restaurant, Address, AppUser, RegisterRestaurantViewModel)
//using System.Linq;
//using System.Security.Claims; // Cần cho User.FindFirstValue
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http; // Cần cho IFormFile
//using System.IO; // Cần cho Path, FileStream
//using Microsoft.AspNetCore.Hosting; // Cần cho IWebHostEnvironment (để lấy đường dẫn wwwroot)
//using PBL3.ViewModels;
//using Microsoft.AspNetCore.Identity;

//namespace PBL3.Controllers
//{
//    public class RestaurantsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _webHostEnvironment; // Để xử lý file upload
//        private readonly UserManager<AppUser> _userManager; // Thêm UserManager

//        public RestaurantsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
//        {
//            _context = context;
//            _webHostEnvironment = webHostEnvironment;
//            _userManager = userManager;
//        }

//        // GET: Restaurants/Create (Hiển thị form đăng ký nhà hàng)
//        public IActionResult Create()
//        {
//            if (!User.Identity.IsAuthenticated)
//            {
//                // Người dùng chưa đăng nhập
//                // Đặt một cờ để JavaScript phía client biết và mở modal
//                ViewData["ShowLoginModal"] = true;
//                ViewData["LoginReturnUrl"] = Url.Action("Create", "Restaurants");
//            }
//            var viewModel = new RegisterRestaurantViewModel();
//            return View(viewModel);
//        }

//        // POST: Restaurants/Create (Xử lý việc tạo nhà hàng mới)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize] // <<== POST action vẫn cần Authorize
//        public async Task<IActionResult> Create(RegisterRestaurantViewModel viewModel)
//        {
//            if (ModelState.IsValid)
//            {
//                // Kiểm tra ModelState trước
//                if (!ModelState.IsValid)
//                {
//                    // Nếu có lỗi, trả về view với viewModel để hiển thị lỗi
//                    return View(viewModel);
//                }

//                // 1. Lấy ID của người dùng hiện tại
//                // Đảm bảo bạn lấy currentUserId đúng cách
//                var currentUserId = _userManager.GetUserId(User); // Cách lấy UserId an toàn hơn
//                if (string.IsNullOrEmpty(currentUserId))
//                {
//                    ModelState.AddModelError(string.Empty, "Không thể xác định người dùng hiện tại.");
//                    return View(viewModel);
//                }

//                // 2. Xử lý upload ảnh (nếu có)
//                string uniqueFileName = null;
//                if (viewModel.MainImageFile != null)
//                {
//                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "restaurants");
//                    if (!Directory.Exists(uploadsFolder)) // Tạo thư mục nếu chưa có
//                    {
//                        Directory.CreateDirectory(uploadsFolder);
//                    }
//                    uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.MainImageFile.FileName;
//                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
//                    using (var fileStream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await viewModel.MainImageFile.CopyToAsync(fileStream);
//                    }
//                    uniqueFileName = "/images/restaurants/" + uniqueFileName; // Lưu đường dẫn tương đối để hiển thị
//                }

//                // 3. Tạo đối tượng Address
//                var address = new Address
//                {
//                    AddressLine1 = viewModel.AddressLine1,
//                    Ward = viewModel.Ward,
//                    District = viewModel.District,
//                    City = viewModel.City,
//                    Country = viewModel.Country,
//                    Latitude = viewModel.Latitude,
//                    Longitude = viewModel.Longitude,
//                    // UserId của Address này sẽ là null vì đây là địa chỉ của nhà hàng, không phải sổ địa chỉ người dùng
//                };
//                _context.Addresses.Add(address);
//                // await _context.SaveChangesAsync(); // Lưu Address trước để có Id (nếu cần ngay)
//                // Hoặc có thể SaveChanges một lần ở cuối

//                // 4. Tạo đối tượng Restaurant
//                var restaurant = new Restaurant
//                {
//                    Name = viewModel.Name,
//                    Description = viewModel.Description,
//                    PhoneNumber = viewModel.PhoneNumber,
//                    Website = viewModel.Website,
//                    OpeningHours = viewModel.OpeningHours,
//                    PriceRange = viewModel.PriceRange,
//                    MainImageUrl = uniqueFileName, // Đường dẫn ảnh đã upload
//                    Status = RestaurantStatus.Open, // Trạng thái mặc định
//                    OwnerId = currentUserId, // Gán chủ sở hữu
//                    // AddressId sẽ được gán sau khi Address được lưu, hoặc EF Core tự xử lý nếu nav prop được set
//                    Address = address // Gán trực tiếp đối tượng Address, EF Core sẽ tự xử lý khóa ngoại khi SaveChanges
//                };

//                _context.Restaurants.Add(restaurant);
//                await _context.SaveChangesAsync(); // Lưu tất cả thay đổi (Address và Restaurant)

//                // TempData["SuccessMessage"] = "Đăng ký nhà hàng thành công!"; // Thông báo thành công (tùy chọn)
//                return RedirectToAction("Details", "Restaurants", new { id = restaurant.Id }); // Chuyển đến trang chi tiết nhà hàng vừa tạo
//            }

//            // Nếu ModelState không hợp lệ, quay lại view với lỗi
//            return View(viewModel);
//        }

//        // GET: Restaurants/Details/5 (Sẽ cần action này để RedirectToAction ở trên hoạt động)
//        [AllowAnonymous] // Cho phép xem chi tiết mà không cần đăng nhập
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var restaurant = await _context.Restaurants
//                .Include(r => r.Address) // Nạp thông tin địa chỉ
//                                         // .Include(r => r.Owner) // Nạp thông tin chủ sở hữu nếu cần hiển thị
//                                         // .Include(r => r.Reviews) // Nạp các review
//                                         // .Include(r => r.Menus).ThenInclude(m => m.MenuSections).ThenInclude(ms => ms.MenuItems) // Nạp menu
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (restaurant == null)
//            {
//                return NotFound();
//            }

//            return View(restaurant); // Cần tạo View Details.cshtml
//        }

//        // Các actions khác (Index, Edit, Delete) sẽ được thêm sau
//    }
//}