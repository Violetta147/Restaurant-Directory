$(function () {
    // --- Code cho Login Modal (giữ nguyên từ trước) ---
    $('body').on('click', '#loginModalButton', function (event) {
        event.preventDefault();
        //button có thể có data-return-url để chuyển hướng sau khi đăng nhập
        // Nếu không có thì sẽ lấy URL hiện tại từ window.location.pathname và window.location.search
        //window.location.pathname là đường dẫn hiện tại, window.location.search là query string
        // Ví dụ: /Home/Index" là window.location.pathname
        // Ví dụ: ?returnUrl=%2FHome%2FIndex là window.location.search
        var returnUrl = $(this).data('return-url') || window.location.pathname + window.location.search;
        var url = "/Account/LoginModal?returnUrl=" + encodeURIComponent(returnUrl);

        if ($('#loginModalInstance').length === 0) {
            $('#loginModalPlaceholder').html('<div class="modal fade" id="loginModalInstance" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true"></div>');
        }
        var $modalInstance = $('#loginModalInstance');
        $modalInstance.load(url, function () {
            var modal = new bootstrap.Modal($modalInstance[0]);
            modal.show();
        });
    });

    $('body').on('submit', '#loginFormModal', function (event) {
        // ... (code submit login form giữ nguyên) ...
        event.preventDefault();
        var form = $(this);
        if (!form.valid()) { return; }
        var submitButton = form.find('button[type="submit"]');
        var originalButtonText = submitButton.html();
        submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...').prop('disabled', true);
        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    if (response.redirectUrl) { window.location.href = response.redirectUrl; }
                    else { window.location.reload(); }
                } else {
                    $('#loginModalInstance').html(response);
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error: ", status, error, xhr.responseText);
                alert("Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
                submitButton.html(originalButtonText).prop('disabled', false);
            }
        });
    });

    $('body').on('hidden.bs.modal', '#loginModalInstance', function () {
        $(this).empty();
    });


    // --- CODE MỚI CHO REGISTER MODAL ---
    $('body').on('click', '#registerModalButton', function (event) {
        event.preventDefault();
        var returnUrl = $(this).data('return-url') || window.location.pathname + window.location.search;
        var url = "/Account/RegisterModal?returnUrl=" + encodeURIComponent(returnUrl); // Action mới

        // Tạo modal container nếu chưa có
        if ($('#registerModalInstance').length === 0) {
            $('#registerModalPlaceholder').html('<div class="modal fade" id="registerModalInstance" tabindex="-1" aria-labelledby="registerModalLabel" aria-hidden="true"></div>');
        }

        var $modalInstance = $('#registerModalInstance');

        // Tải nội dung form vào modal
        $modalInstance.load(url, function () {
            var modal = new bootstrap.Modal($modalInstance[0]);
            modal.show();
        });
    });

    // Xử lý submit form đăng ký bằng AJAX
    $('body').on('submit', '#registerFormModal', function (event) {
        event.preventDefault();
        var form = $(this);

        if (!form.valid()) { // Cần jquery.validate.js
            return;
        }

        var submitButton = form.find('button[type="submit"]');
        var originalButtonText = submitButton.html();
        submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...').prop('disabled', true);

        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function (response) {
                if (response.success && response.redirectUrl) {
                    // Đăng ký bước 1 thành công, chuyển hướng đến trang hoàn tất
                    window.location.href = response.redirectUrl;
                } else {
                    // Nếu có lỗi ngay ở bước 1 (ví dụ email đã tồn tại), server trả về partial view với lỗi
                    $('#registerModalInstance').html(response);
                    // Parse lại validation cho form mới
                    var newForm = $('#registerFormModal');
                    if (newForm.length && typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
                        newForm.removeData('validator');
                        newForm.removeData('unobtrusiveValidation');
                        $.validator.unobtrusive.parse(newForm);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error: ", status, error, xhr.responseText);
                alert("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
                submitButton.html(originalButtonText).prop('disabled', false);
            }
        });
    });

    // Tùy chọn: Dọn dẹp modal đăng ký khi nó bị đóng
    $('body').on('hidden.bs.modal', '#registerModalInstance', function () {
        $(this).empty();
    });


    // --- NAVBAR LOCATION & GEOCODING --- 
    const mapboxTokenMeta = document.querySelector('meta[name="mapbox-token"]');
    const MAPBOX_ACCESS_TOKEN = mapboxTokenMeta ? mapboxTokenMeta.content : null;

    if (!MAPBOX_ACCESS_TOKEN) {
        console.warn('Mapbox Access Token not found. Geocoding features will be disabled.');
    }

    const navbarLatInput = $('#navbarLat');
    const navbarLngInput = $('#navbarLng');
    const navbarLocationInput = $('#navbarLocationInput');
    const navbarSearchForm = $('#navbarSearchForm');

    // 1. "Use Current Location" button
    $('#navbarUseCurrentLocation').on('click', function() {
        if (!navigator.geolocation) {
            alert('Geolocation is not supported by your browser.');
            return;
        }

        navigator.geolocation.getCurrentPosition(function(position) {
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            
            navbarLatInput.val(lat.toFixed(6));
            navbarLngInput.val(lng.toFixed(6));
            navbarLocationInput.val('Current Location'); // Placeholder text

            // Optional: Reverse geocode to get address string
            if (MAPBOX_ACCESS_TOKEN) {
                fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lng},${lat}.json?access_token=${MAPBOX_ACCESS_TOKEN}&limit=1&language=vi`)
                    .then(response => response.json())
                    .then(data => {
                        if (data.features && data.features.length > 0) {
                            navbarLocationInput.val(data.features[0].place_name_vi || data.features[0].place_name);
                        }
                    })
                    .catch(error => console.error('Error reverse geocoding:', error));
            }
            // Optional: Automatically submit the form
            // navbarSearchForm.submit(); 
        }, function(error) {
            let message = 'Error getting location: ';
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    message += "User denied the request for Geolocation.";
                    break;
                case error.POSITION_UNAVAILABLE:
                    message += "Location information is unavailable.";
                    break;
                case error.TIMEOUT:
                    message += "The request to get user location timed out.";
                    break;
                case error.UNKNOWN_ERROR:
                    message += "An unknown error occurred.";
                    break;
            }
            alert(message);
        });
    });

    // 2. Geocode address input on form submit
    if (navbarSearchForm.length > 0) {
        navbarSearchForm.on('submit', async function(event) {
            const locationText = navbarLocationInput.val().trim();
            // Store original values from hidden inputs for logic, but clear them for URL if 'Current Location'
            const originalLatVal = navbarLatInput.val();
            const originalLngVal = navbarLngInput.val();

            if (locationText === 'Current Location') {
                // Clear the input fields so they don't go into the URL for 'Current Location' searches.
                // The server-side logic will use viewModel.UserLat/UserLng instead.
                navbarLatInput.val('');
                navbarLngInput.val('');
            }

            // Only geocode if location text is present AND lat/lng are not already set (e.g., by 'use current location')
            // Use originalLatVal and originalLngVal for this check, as navbarLatInput might be cleared now.
            if (MAPBOX_ACCESS_TOKEN && locationText && locationText !== 'Current Location' && (!originalLatVal || !originalLngVal)) {
                event.preventDefault(); // Prevent default submission, we'll submit manually after geocoding
                
                try {
                    const response = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(locationText)}.json?access_token=${MAPBOX_ACCESS_TOKEN}&country=VN&language=vi&limit=1`);
                    const data = await response.json();

                    if (data.features && data.features.length > 0) {
                        const coordinates = data.features[0].center;
                        navbarLngInput.val(coordinates[0].toFixed(6)); // Mapbox returns [lng, lat]
                        navbarLatInput.val(coordinates[1].toFixed(6));
                    } else {
                        // No coordinates found, clear any potentially stale lat/lng
                        navbarLatInput.val('');
                        navbarLngInput.val('');
                        console.warn('Geocoding: No results found for: ', locationText);
                        // Optionally alert the user: alert('Could not find coordinates for the entered location. Searching by text only.');
                    }
                } catch (error) {
                    console.error('Error during geocoding:', error);
                    // Clear potentially stale lat/lng in case of error
                    navbarLatInput.val('');
                    navbarLngInput.val('');
                    // Optionally alert the user: alert('An error occurred while trying to find coordinates. Searching by text only.');
                }
                
                // Submit the form programmatically after attempting geocoding
                this.submit(); 

            } else if (locationText === 'Current Location' && (!originalLatVal || !originalLngVal)) {
                // This case might happen if 'Use Current Location' was clicked but geolocation failed or was slow,
                // and user submitted before lat/lng were populated. 
                // We could try to re-trigger geolocation or just let it submit without lat/lng.
                // For now, let it submit. If lat/lng are missing, server handles it.
            }
            // If lat/lng are already set, or no locationText, or no token, it will submit normally without async geocoding here.
        });
    }
});