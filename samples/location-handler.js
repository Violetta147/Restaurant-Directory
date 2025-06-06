/**
 * location-handler.js - Xử lý geo-location và tìm kiếm địa điểm 
 * Tạo ngày 04/06/2025
 */

$(function () {
    // --- LOCATION & GEOCODING --- 
    const mapboxTokenMeta = document.querySelector('meta[name="mapbox-token"]');
    const MAPBOX_ACCESS_TOKEN = mapboxTokenMeta ? mapboxTokenMeta.content : null;
    if (!MAPBOX_ACCESS_TOKEN) {
        console.warn('Mapbox Access Token not found. Geocoding features will be disabled.');
    }
    
    // Cache DOM elements
    const navbarLatInput = $('#navbarLat');
    const navbarLngInput = $('#navbarLng');
    const navbarLocationInput = $('#navbarLocationInput');
    const navbarSearchForm = $('#navbarSearchForm');
    const locationSuggestion = $('#location-suggestion');
    
    // Đảm bảo autocomplete luôn tắt
    $('#navbarSearchForm').attr('autocomplete', 'off');
    navbarLocationInput.attr('autocomplete', 'off');
    
    // Đổi tên trường để ngăn chặn autocomplete
    navbarLocationInput.attr('name', 'randomloc-' + Math.floor(Math.random() * 1000000));
    
    // Xử lý hiển thị dropdown khi focus
    navbarLocationInput.on('focus', function() {
        locationSuggestion.show();
    });
    
    // Đóng dropdown khi click ra ngoài
    $(document).on('click', function(e) {
        if (!navbarLocationInput.is(e.target) && 
            !locationSuggestion.is(e.target) && 
            locationSuggestion.has(e.target).length === 0) {
            locationSuggestion.hide();
        }
    });
    
    // Xử lý nút "Sử dụng vị trí hiện tại"
    $('#navbarUseCurrentLocation').on('click', function(e) {
        e.stopPropagation();
        
        if (!navigator.geolocation) {
            alert('Geolocation không được hỗ trợ bởi trình duyệt của bạn.');
            return;
        }
        
        // Thay đổi văn bản của nút để hiển thị trạng thái đang xử lý
        const originalButtonText = $(this).html();
        $(this).html('<i class="fas fa-spinner fa-spin"></i> Đang xác định vị trí...');
        
        // Ẩn dropdown
        locationSuggestion.hide();
        
        // Lấy vị trí GPS
        navigator.geolocation.getCurrentPosition(
            function(position) {
                // Thành công - lấy được tọa độ
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                
                // Lưu tọa độ vào form inputs
                navbarLatInput.val(lat.toFixed(6));
                navbarLngInput.val(lng.toFixed(6));
                
                // Đặt text hiển thị là "Vị trí hiện tại" đối với người dùng
                navbarLocationInput.val('Vị trí hiện tại');
                
                // Khôi phục văn bản nút
                $('#navbarUseCurrentLocation').html(originalButtonText);
                
                // Reverse geocode để lấy địa chỉ thực
                if (MAPBOX_ACCESS_TOKEN) {
                    fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lng},${lat}.json?access_token=${MAPBOX_ACCESS_TOKEN}&limit=1&language=vi`)
                        .then(response => response.json())
                        .then(data => {
                            if (data.features && data.features.length > 0) {
                                // Lưu địa chỉ thực vào data attribute để dùng khi submit
                                const realAddress = data.features[0].place_name_vi || data.features[0].place_name;
                                navbarLocationInput.data('realAddress', realAddress);
                            }
                        })
                        .catch(error => console.error('Error reverse geocoding:', error));
                }
            },
            function(error) {
                // Lỗi - không lấy được vị trí
                $('#navbarUseCurrentLocation').html(originalButtonText);
                
                let message = 'Lỗi khi lấy vị trí: ';
                switch(error.code) {
                    case error.PERMISSION_DENIED:
                        message += "Quyền truy cập vị trí đã bị từ chối.";
                        break;
                    case error.POSITION_UNAVAILABLE:
                        message += "Thông tin vị trí không khả dụng.";
                        break;
                    case error.TIMEOUT:
                        message += "Yêu cầu lấy vị trí người dùng đã hết thời gian.";
                        break;
                    case error.UNKNOWN_ERROR:
                        message += "Đã xảy ra lỗi không xác định.";
                        break;
                }
                alert(message);
            }
        );
    });
    
    // Xử lý khi form được submit
    if (navbarSearchForm.length > 0) {
        navbarSearchForm.on('submit', async function(event) {
            // Đổi tên field về "location" để controller có thể xử lý
            navbarLocationInput.attr('name', 'location');
            
            const locationText = navbarLocationInput.val().trim();
            
            // Nếu là "Vị trí hiện tại", sử dụng địa chỉ thực từ data attribute
            if (locationText === 'Vị trí hiện tại') {
                const realAddress = navbarLocationInput.data('realAddress');
                if (realAddress) {
                    // Sử dụng địa chỉ thực trong URL thay vì "Vị trí hiện tại"
                    navbarLocationInput.val(realAddress);
                }
                // Giữ nguyên tọa độ lat/lng
            }
            // Cho phép form submit như bình thường
            else if (locationText && locationText !== '' && MAPBOX_ACCESS_TOKEN) {
                // Nếu không có lat/lng nhưng có địa chỉ, thực hiện geocoding trước khi submit
                if (!navbarLatInput.val() || !navbarLngInput.val()) {
                    try {
                        event.preventDefault(); // Ngăn form submit ngay lập tức
                        
                        const response = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(locationText)}.json?access_token=${MAPBOX_ACCESS_TOKEN}&country=VN&language=vi&limit=1`);
                        const data = await response.json();
                        
                        if (data.features && data.features.length > 0) {
                            const coordinates = data.features[0].center;
                            navbarLngInput.val(coordinates[0].toFixed(6)); // Mapbox trả về [lng, lat]
                            navbarLatInput.val(coordinates[1].toFixed(6));
                        }
                        
                        // Submit form sau khi geocoding
                        this.submit();
                    } catch (error) {
                        console.error('Error during geocoding:', error);
                        // Vẫn submit form dù có lỗi
                        return true;
                    }
                }
            }
        });
    }
});
