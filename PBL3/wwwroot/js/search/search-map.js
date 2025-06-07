// Search Map Functionality
let map;
let markers = [];
let currentPopup = null;
let mapisDragging = false;
let mapMoved = false;

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing search map...');
    initializeMap();
});

function initializeMap() {
    // Get map configuration from hidden inputs
    const mapboxToken = document.getElementById('mapbox-token')?.value;
    const initialLat = parseFloat(document.getElementById('map-initial-lat')?.value || '16.047079');
    //log javascript to check if initial latitude is valid
    console.log('Initial latitude:', isNaN(initialLat) ? 'Invalid' : initialLat);
    //log javascript to check if initial longitude is valid
    const initialLng = parseFloat(document.getElementById('map-initial-lng')?.value || '108.206230');
    console.log('Initial longitude:', isNaN(initialLng) ? 'Invalid' : initialLng);
    
    if (!mapboxToken) {
        console.warn('Mapbox token not found. Map will not be initialized.');
        showMapError('Map configuration missing');
        return;
    }
      // No need to remove scale controls - they won't be added in the first place

    console.log('Map config:', { lat: initialLat, lng: initialLng });

    try {
        // Set Mapbox access token
        mapboxgl.accessToken = mapboxToken;
        //log mapboxgltoken to check if it is valid
        console.log('Mapbox access token set:', mapboxgl.accessToken);

        // Initialize map
        map = new mapboxgl.Map({
            container: 'mapbox-container',
            style: 'mapbox://styles/mapbox/streets-v12',
            center: [initialLng, initialLat],
            zoom: 12,
            language: 'vi'
        });        // Add navigation controls
        map.addControl(new mapboxgl.NavigationControl(), 'top-right');
        //add user location control
        if (navigator.geolocation) {
            map.addControl(new mapboxgl.GeolocateControl({
                positionOptions: {
                    enableHighAccuracy: true
                },
                trackUserLocation: true,
                showUserLocation: true,
                fitBoundsOptions: {
                    maxZoom: 15
                }
            }), 'top-right');
        } else {
            console.warn('Geolocation not supported by this browser');
            showMapError('Geolocation not supported');
        }
        // Wait for map to load before adding markers
        map.on('load', function() {
            console.log('Map loaded successfully');
            loadRestaurantsFromList();
        });

        // Hide POI labels when style loads
        map.on('style.load', () => {
            map.getStyle().layers.forEach(function(layer) {
                if (layer.id.includes('poi') || layer.id.includes('poi-label')) {
                    map.setLayoutProperty(layer.id, 'visibility', 'none');
                }
            });
            console.log('POI labels hidden');
        });

        map.on('error', function(e) {
            console.error('Map error:', e);
            showMapError('Failed to load map');
        });

    } catch (error) {
        console.error('Error initializing map:', error);
        showMapError('Map initialization failed');
    }
}

function showMapError(message) {
    const mapContainer = document.getElementById('mapbox-container');
    if (mapContainer) {
        mapContainer.innerHTML = `
            <div class="d-flex align-items-center justify-content-center h-100 text-muted">
                <div class="text-center">
                    <i class="bi bi-geo-alt display-4 mb-3"></i>
                    <p class="mb-0">${message}</p>
                    <button class="btn btn-outline-primary btn-sm mt-2" onclick="location.reload()">
                        Thử lại
                    </button>
                </div>
            </div>`;
    }
}

// Load restaurants from the current page and add markers
function loadRestaurantsFromList() {
    if (!map) {
        console.warn('Map not initialized');
        return;
    }

    console.log('Loading restaurants from list...');
    
    // Clear existing markers
    clearMarkers();

    // Get restaurant cards from the page
    const restaurantCards = document.querySelectorAll('.restaurant-item');
    console.log('Found restaurant cards:', restaurantCards.length);

    const bounds = new mapboxgl.LngLatBounds();
    let markersAdded = 0;

    restaurantCards.forEach((card, index) => {        const lat = parseFloat(card.dataset.lat);
        //log javascript to check if latitude is valid
        console.log(`Card ${index + 1} latitude:`, isNaN(lat) ? 'Invalid' : lat);
        const lng = parseFloat(card.dataset.lng);
        const name = card.dataset.name || card.querySelector('.card-title')?.textContent || 'Unknown Restaurant';
        const rating = card.dataset.rating || card.querySelector('.fw-bold')?.nextElementSibling?.textContent || 'N/A';
        const reviewCount = card.dataset.reviewCount || card.querySelector('.text-muted.small')?.textContent.match(/\((\d+) reviews\)/)?.[1] || '0';
        const address = card.dataset.address || card.querySelector('.bi-geo-alt')?.parentNode?.textContent.trim() || '';
        const id = card.dataset.id;

        if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
            console.log(`Adding marker for: ${name} at (${lat}, ${lng})`);
            
            // Create marker element
            const markerElement = createMarkerElement(index + 1, rating);
              // Create marker
            const marker = new mapboxgl.Marker({
                element: markerElement,
                anchor: 'center' // Use center anchor to avoid positioning issues
            })
                .setLngLat([lng, lat])
                .addTo(map);

            // Create popup content
            const popupContent = createPopupContent(name, rating, reviewCount, address, id);
            
            // Create popup
            const popup = new mapboxgl.Popup({
                closeButton: false,
                closeOnClick: false,
            }).setHTML(popupContent);            // Add hover events to marker for popup display
            markerElement.addEventListener('mouseenter', function(e) {
                e.stopPropagation();
                
                // Close current popup if exists
                if (currentPopup) {
                    currentPopup.remove();
                    console.log('Closed previous popup');
                }
                
                // Store the original marker position for reference
                const markerPosition = marker.getLngLat();
                
                // Show new popup with explicit position
                popup.setLngLat([markerPosition.lng, markerPosition.lat]);
                popup.addTo(map);
                currentPopup = popup;
            });
            
            // Add click event to marker too (alternative to hovering)
            markerElement.addEventListener('click', function(e) {
                e.stopPropagation();
                
                // Close current popup if exists
                if (currentPopup) {
                    currentPopup.remove();
                    console.log('Closed previous popup');
                }
                
                // Store the original marker position for reference
                const markerPosition = marker.getLngLat();
                
                // Show new popup with explicit position
                popup.setLngLat([markerPosition.lng, markerPosition.lat]);
                popup.addTo(map);
                currentPopup = popup;
            });
            
            // Remove popup when mouse leaves the marker
            markerElement.addEventListener('mouseleave', function() {
                // Use a variable to track if mouse is over popup
                let isOverPopup = false;
                
                // Add listener to popup element (if available)
                const popupElement = popup.getElement();
                if (popupElement) {
                    popupElement.addEventListener('mouseenter', function() {
                        isOverPopup = true;
                    });
                    
                    popupElement.addEventListener('mouseleave', function() {
                        isOverPopup = false;
                        setTimeout(() => {
                            if (!isOverPopup && currentPopup) {
                                currentPopup.remove();
                                currentPopup = null;
                                console.log('Closed popup on popup mouse leave');
                            }
                        }, 300);
                    });
                }
                
                setTimeout(() => {
                    if (!isOverPopup && currentPopup) {
                        currentPopup.remove();
                        currentPopup = null;
                        console.log('Closed popup on marker mouse leave');
                    }
                }, 300); // Small delay to make UX smoother
            });            // Make sure id is stored as a string for consistent comparisons
            markers.push({ marker, popup, id: id.toString() });
            bounds.extend([lng, lat]);
            markersAdded++;
        }
    });

    console.log(`Added ${markersAdded} markers to map`);

    // Fit map to show all markers if any were added
    if (markersAdded > 0) {
        try {
            map.fitBounds(bounds, {
                padding: { top: 50, bottom: 50, left: 50, right: 50 },
                maxZoom: 15
            });
        } catch (error) {
            console.warn('Error fitting bounds:', error);
        }
    }
}

function createMarkerElement(number, rating) {
    const element = document.createElement('div');
    element.className = 'custom-marker';
    element.style.cssText = `
        width: 40px;
        height: 40px;
        background-color: #dc3545; /* Trở lại màu đỏ mặc định */
        border: 2px solid white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
        font-weight: bold;
        font-size: 14px;
        cursor: pointer;
        box-shadow: 0 2px 4px rgba(0,0,0,0.3);
    `;
    
    element.textContent = number;
    
    // Add hover effect without using transform
    element.addEventListener('mouseenter', function() {
        // Change appearance without transform
        this.style.width = '44px';
        this.style.height = '44px';
        this.style.backgroundColor = '#00ff7f'; /* Màu xanh neon khi hover */
        this.style.boxShadow = '0 3px 5px rgba(0,0,0,0.5), 0 0 8px #00ff7f'; /* Thêm glow effect */
        this.style.zIndex = 10; // Bring to front
    });
    
    element.addEventListener('mouseleave', function() {
        // Reset appearance
        this.style.width = '40px';
        this.style.height = '40px';
        this.style.backgroundColor = '#dc3545'; /* Trở lại màu đỏ mặc định */
        this.style.boxShadow = '0 2px 4px rgba(0,0,0,0.3)';
        this.style.zIndex = ''; // Reset z-index
    });
    
    return element;
}

function createPopupContent(name, rating, reviewCount, address, id) {
    return `
        <div class="restaurant-popup" style="min-width: 200px;">
            <h6 class="mb-2 fw-bold">${name}</h6>
            <div class="d-flex align-items-center mb-2">
                <span class="text-warning me-1">★</span>
                <span class="fw-bold me-1">${rating}</span>
                <small class="text-muted">(${reviewCount} đánh giá)</small>
            </div>
            <p class="small text-muted mb-2">${address}</p>
            <div class="d-flex gap-2">
                <a href="/Restaurant/Details/${id}" class="btn btn-primary btn-sm">
                    Xem chi tiết
                </a>
                <button class="btn btn-outline-secondary btn-sm" onclick="centerMapOnRestaurant(${id})">
                    Phóng to
                </button>
            </div>
        </div>
    `;
}

function clearMarkers() {
    markers.forEach(({ marker, popup }) => {
        if (popup) popup.remove();
        if (marker) marker.remove();
    });
    markers = [];
    
    if (currentPopup) {
        currentPopup.remove();
        currentPopup = null;
    }
}

// Reload map functionality
document.addEventListener('DOMContentLoaded', function() {
    const reloadButton = document.getElementById('reload-map-button');
    if (reloadButton) {
        reloadButton.addEventListener('click', function() {
            this.style.display = 'none';
            initializeMap();
        });
    }
});

// Handle window resize
window.addEventListener('resize', function() {
    if (map) {
        setTimeout(() => {
            map.resize();
        }, 100);
    }
});

// Common function to focus map on a restaurant
function focusMapOnRestaurant(restaurantId, zoomLevel = 15, duration = 1000) {
    const card = document.querySelector(`.restaurant-item[data-id="${restaurantId}"]`);
    if (!card || !map) {
        console.warn(`Card for restaurant ID ${restaurantId} not found or map not initialized`);
        return false;
    }
    
    const lat = parseFloat(card.dataset.lat);
    const lng = parseFloat(card.dataset.lng);
    
    if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
        console.log(`Centering map on restaurant: ${restaurantId} at (${lat}, ${lng})`);
        
        // Convert restaurantId to string to ensure consistent comparison
        const strId = restaurantId.toString();
        console.log(`Looking for marker with ID: ${strId} (type: ${typeof strId})`);
        
        // Log all markers for debugging
        console.log('All markers:', markers.map(m => ({id: m.id, type: typeof m.id})));
        
        // Find the marker and show its popup
        const matchingMarker = markers.find(m => m.id == strId);
        
        // Move map to restaurant location
        map.flyTo({
            center: [lng, lat],
            zoom: zoomLevel,
            duration: duration
        });
        
        // Wait for map movement to complete before showing popup
        setTimeout(() => {
            if (matchingMarker) {
                console.log('Found matching marker, showing popup');
                
                // Close any existing popup
                if (currentPopup) {
                    currentPopup.remove();
                }
                
                // Set position and add popup to map
                if (matchingMarker.popup) {
                    matchingMarker.popup.setLngLat([lng, lat]);
                    matchingMarker.popup.addTo(map);
                    currentPopup = matchingMarker.popup;
                }
            } else {
                console.warn(`No marker found for restaurant ID: ${strId}`);
            }
        }, duration); // Wait for map animation to complete
        
        return true;
    }
    
    return false;
}

// Initialize restaurant card handlers
function initializeRestaurantCardHandlers() {
    const restaurantCards = document.querySelectorAll('.restaurant-item');
    console.log('Finding restaurant cards to attach handlers...');
    
    restaurantCards.forEach(card => {
        // Get restaurant ID from card data attribute
        const restaurantId = card.dataset.id;
        if (!restaurantId) {
            console.warn('Restaurant card missing ID attribute:', card);
            return;
        }
        
        // Remove existing listeners to avoid duplicates
        const newCard = card.cloneNode(true);
        card.replaceWith(newCard);
        
        // Add click event to restaurant cards
        newCard.addEventListener('click', function(event) {
            // Only handle clicks on the card itself, not on links or buttons
            if (event.target.tagName.toLowerCase() === 'a' || 
                event.target.tagName.toLowerCase() === 'button') {
                return; // Let the link or button handle the click
            }
            
            const restaurantId = this.dataset.id;
            console.log(`Card clicked for restaurant ID: ${restaurantId} (type: ${typeof restaurantId})`);
            
            // Debug info
            console.log('Available markers:', markers.map(m => m.id));
            
            // First update the markers variable to ensure all markers have string IDs
            markers = markers.map(m => ({
                ...m, 
                id: m.id.toString() // Ensure all IDs are strings
            }));
            
            // Change the marker appearance to neon green
            const matchingMarker = markers.find(m => m.id == restaurantId.toString());
            if (matchingMarker) {
                // Get the marker element
                const markerElement = matchingMarker.marker.getElement();
                if (markerElement) {
                    // Apply neon green style
                    markerElement.style.width = '44px';
                    markerElement.style.height = '44px';
                    markerElement.style.backgroundColor = '#00ff7f'; // Màu xanh neon
                    markerElement.style.boxShadow = '0 3px 5px rgba(0,0,0,0.5), 0 0 8px #00ff7f'; // Thêm hiệu ứng phát sáng
                    markerElement.style.zIndex = 10;
                    
                    // Reset style after 3 seconds
                    setTimeout(() => {
                        markerElement.style.width = '40px';
                        markerElement.style.height = '40px';
                        markerElement.style.backgroundColor = '#dc3545'; // Red default color
                        markerElement.style.boxShadow = '0 2px 4px rgba(0,0,0,0.3)';
                        markerElement.style.zIndex = '';
                    }, 3000);
                }
            }
            
            // Focus map on restaurant and show popup
            const success = focusMapOnRestaurant(restaurantId);
            
            // Force-show popup if not already shown
            setTimeout(() => {
                const matchingMarker = markers.find(m => m.id == restaurantId.toString());
                if (matchingMarker && matchingMarker.popup) {
                    // Close any existing popup
                    if (currentPopup) {
                        currentPopup.remove();
                    }
                    
                    const lat = parseFloat(this.dataset.lat);
                    const lng = parseFloat(this.dataset.lng);
                    
                    if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
                        matchingMarker.popup.setLngLat([lng, lat]);
                        matchingMarker.popup.addTo(map);
                        currentPopup = matchingMarker.popup;
                    }
                }
            }, 1200); // Wait a bit after the map animation
            
            // Log result
            console.log(`Focus on restaurant ${restaurantId} result: ${success ? 'success' : 'failed'}`);
        });
    });
    console.log('Restaurant card handlers initialized with', restaurantCards.length, 'cards');
}

// Initialize card handlers when page loads
document.addEventListener('DOMContentLoaded', function() {
    // Wait a bit for the map and markers to be ready
    setTimeout(initializeRestaurantCardHandlers, 500);
});

// Function called from popup "Trung tâm" button
function centerMapOnRestaurant(restaurantId) {
    // Convert ID to string to ensure consistent comparison
    const strId = restaurantId.toString();
    console.log(`Centering map on restaurant ID: ${strId}`);
    
    // Make sure all markers have string IDs for consistent comparison
    markers = markers.map(m => ({...m, id: m.id.toString()}));
    
    const success = focusMapOnRestaurant(strId, 16, 2000);
    console.log(`Centered map on restaurant ID: ${strId}, success: ${success}`);
}

// Global function to be called after AJAX updates
window.loadRestaurantsFromList = loadRestaurantsFromList;
window.centerMapOnRestaurant = centerMapOnRestaurant;
window.initializeRestaurantCardHandlers = initializeRestaurantCardHandlers;

