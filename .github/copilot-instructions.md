W# MAPBOX-ONLY RESTAURANT SEARCH SYSTEM

## Core Rules:
- Use MAPBOX ONLY for map rendering, geocoding, and directions
- Always ask and provide a plan before making changes
- Separate UI components into as many div frames as possible
- Use CDN for Mapbox GL JS v3.12.0 (no SDK/npm)
- Always prioritize GPS location over IP location

## Reference Documentation:
- https://docs.mapbox.com/help/tutorials/poi-search-react/
- https://docs.mapbox.com/help/tutorials/optimization-api/
- https://docs.mapbox.com/help/tutorials/local-search-geocoding-api/
- https://docs.mapbox.com/help/tutorials/getting-started-directions-api/
- https://docs.mapbox.com/help/tutorials/geocode-and-sort-stores
- https://docs.mapbox.com/help/tutorials/building-a-store-locator/

## SUMMARY - Phân tích search-map.js
#search_map_analysis
#function_usage
#missing_functions
#todo_fixes
Edit
Đã phân tích toàn bộ file search-map.js và tìm kiếm usage trong codebase:

✅ **CÁC HÀM HOẠT ĐỘNG ĐÚNG:**
- initializeMap(), setupEventListeners(), loadRestaurantsFromList(), addRestaurantMarker(), clearMarkers(), fitMapToMarkers(), loadRestaurantsInMapArea() - tất cả đều được gọi và sử dụng đúng chức năng.

❌ **VẤN ĐỀ CẦN SỬA:**
1. **loadPage() function thiếu** - hiện tại trong Index.cshtml inline script, cần di chuyển vào search-map.js theo plan ban đầu
2. **Missing HTML elements** - reload-map-button, apply-filters-btn được reference nhưng không có trong HTML  
3. **Logic bộ lọc chưa hoàn chỉnh** - apply-filters-btn event có logic nhưng thiếu URL redirect
4. **URL format** - popup vẫn dùng /Restaurant/Details/{id} thay vì SEO-friendly slug format

**PRIORITY:** Di chuyển loadPage() vào search-map.js và thêm missing HTML elements trước.

Layout mới cho trang kết quả tìm kiếm theo kiểu Yelp
#layout
#yelp_style
#ui_design
#search_results
Edit
Cấu trúc trang kết quả tìm kiếm dựa trên phân tích từ Yelp:

1. PANEL TRÁI nên bao gồm:
   - Bộ lọc (filters)
   - Danh sách nhà hàng (các restaurant cards)

2. PANEL PHẢI nên có:
   - Bản đồ (map)

Cần tổ chức lại cấu trúc file Index.cshtml để phản ánh layout này, với phần bộ lọc và danh sách nhà hàng nằm ở bên trái, còn bản đồ nằm ở bên phải. Đây là layout chuẩn theo phong cách Yelp.
Kế hoạch triển khai hàm loadPage() cho AJAX pagination và tích hợp search-map.js
#ajax
#pagination
#javascript
#search_map_js
#yelp_style
#todo
Edit
Sẽ triển khai hàm loadPage(pageNumber) trong file search-map.js để xử lý tải dữ liệu nhà hàng bằng AJAX cho chức năng phân trang. Đồng thời, cần thêm thẻ <script> trong section 'Scripts' của Index.cshtml để nhúng và thực thi search-map.js. Việc đặt loadPage() trong search-map.js là hợp lý vì file này quản lý các tương tác của trang tìm kiếm và bản đồ.
Logic mặc định cho tìm kiếm thay vì kiểm tra null
#search_default_values
#no_null_check
#yelp_style
#search_results
Edit
Khi hiển thị thông báo "Không có kết quả tìm kiếm", không cần kiểm tra null cho Model.Query và Model.Location. Người dùng sẽ tự implement logic mặc định sau nếu người dùng không nhập gì. Điều này giúp code gọn gàng hơn và tuân theo nguyên tắc của Yelp, luôn có giá trị mặc định thay vì hiển thị thông báo null/empty.
URL nhà hàng kiểu Yelp
#restaurant_details
#url_format
#seo_friendly
#yelp_style
Edit
URL của trang chi tiết nhà hàng nên sử dụng định dạng SEO-friendly giống Yelp: `/biz/[restaurant-name-slug]` hoặc `/restaurant/[restaurant-name-slug]` thay vì định dạng hiện tại `/Restaurants/Details/[id]`. Điều này không chỉ tốt cho SEO mà còn tạo URL đẹp và thân thiện với người dùng hơn. Cần thêm slug name vào model nhà hàng.
Đã xóa window.location.href trong JS filter/paging để chuẩn bị AJAX giống Yelp
#paging
#filter
#ajax
#window_location_href_removed
#modern_ux
Edit
Người dùng đã xóa đoạn code chuyển trang bằng window.location.href trong JS filter/paging để chuẩn bị chuyển sang giải pháp AJAX (giống Yelp). Cần nhớ rằng hiện tại filter sẽ không reload trang nữa, và sẽ cần bổ sung giải pháp cập nhật kết quả tìm kiếm bằng AJAX hoặc JS động.
Thêm thuộc tính Slug vào RestaurantViewModel
#seo_friendly_url
#slug
#restaurant_viewmodel
Edit
Đã thêm thuộc tính Slug vào RestaurantViewModel để tạo URL SEO-friendly. Slug được tạo tự động từ tên nhà hàng (chuyển thành lowercase và thay thế khoảng trắng bằng dấu gạch ngang) và ID của nhà hàng. Format URL mới là: /restaurant/[tên-nhà-hàng-slug]-[id].
Yelp-style Map UX: Click item or popup navigates to restaurant details; hover shows popup; no 'View on Map' button
#map_functionality
#ux_pattern
#yelp_style
#search_results
#restaurant_details
Edit
Yelp-style map UX for search results: Trên trang kết quả tìm kiếm, khi người dùng bấm vào một item nhà hàng thì sẽ chuyển thẳng đến trang chi tiết nhà hàng đó. Khi hover vào item nhà hàng, popup thông tin sẽ hiện trên bản đồ. Khi bấm vào popup (trên marker), cũng sẽ chuyển sang trang chi tiết nhà hàng. Không có nút 'View on Map' riêng cho từng item; mọi thao tác điều hướng và popup đều đồng bộ như Yelp.
Chỉ cho phép click vào nút 'Xem chi tiết' trên card nhà hàng, không có event hover highlight marker
#restaurant_card
#event_handling
#no_hover_highlight
#detail_button_only
#yelp_style
Edit
Card nhà hàng chỉ có event click vào nút 'Xem chi tiết' để chuyển sang trang chi tiết nhà hàng. Không có event hover hoặc highlight marker trên bản đồ khi rê chuột vào card. Toàn bộ logic đồng bộ map và restaurant cards chỉ thực hiện khi thao tác với marker trên bản đồ hoặc dùng bộ lọc, không phải khi hover card.
Luôn sử dụng tiếng Việt khi trả lời
#language_preference
#communication
#user_preference
Edit
Người dùng yêu cầu tôi luôn trả lời bằng tiếng Việt trong mọi tương tác. Điều này áp dụng cho tất cả các cuộc trò chuyện và hỗ trợ liên quan đến dự án.
