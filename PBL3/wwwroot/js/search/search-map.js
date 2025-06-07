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

    restaurantCards.forEach((card, index) => {
        const lat = parseFloat(card.dataset.lat);
        //log javascript to check if latitude is valid
        console.log(`Card ${index + 1} latitude:`, isNaN(lat) ? 'Invalid' : lat);
        const lng = parseFloat(card.dataset.lng);
        const name = card.dataset.name || card.querySelector('.restaurant-name')?.textContent || 'Unknown Restaurant';
        const rating = card.dataset.rating || card.querySelector('.rating-value')?.textContent || 'N/A';
        const reviewCount = card.dataset.reviewCount || card.querySelector('.review-count')?.textContent || '0';
        const address = card.dataset.address || card.querySelector('.restaurant-address')?.textContent || '';
        const id = card.dataset.id;

        if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
            console.log(`Adding marker for: ${name} at (${lat}, ${lng})`);
            
            // Create marker element
            const markerElement = createMarkerElement(index + 1, rating);
            
            // Create marker
            const marker = new mapboxgl.Marker(markerElement)
                .setLngLat([lng, lat])
                .addTo(map);

            // Create popup content
            const popupContent = createPopupContent(name, rating, reviewCount, address, id);
            
            // Create popup
            const popup = new mapboxgl.Popup({
                closeButton: true,
                closeOnClick: false,
                offset: [0, -15]
            }).setHTML(popupContent);

            // Add click event to marker
            markerElement.addEventListener('click', function(e) {
                e.stopPropagation();
                
                // Close current popup if exists
                if (currentPopup) {
                    currentPopup.remove();
                }
                
                // Show new popup
                popup.addTo(map);
                currentPopup = popup;
                
                // Center map on marker
                map.easeTo({
                    center: [lng, lat],
                    zoom: Math.max(map.getZoom(), 15),
                    duration: 1000
                });
            });

            markers.push({ marker, popup, id });
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
        background-color: #dc3545;
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
        transition: transform 0.2s ease;
    `;
    
    element.textContent = number;
    
    // Add hover effect
    element.addEventListener('mouseenter', function() {
        this.style.transform = 'scale(1.1)';
    });
    
    element.addEventListener('mouseleave', function() {
        this.style.transform = 'scale(1)';
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
                    Trung tâm
                </button>
            </div>
        </div>
    `;
}

function centerMapOnRestaurant(restaurantId) {
    const card = document.querySelector(`.restaurant-item[data-id="${restaurantId}"]`);
    if (card && map) {
        const lat = parseFloat(card.dataset.lat);
        const lng = parseFloat(card.dataset.lng);
        
        if (lat && lng) {
            map.flyTo({
                center: [lng, lat],
                zoom: 16,
                duration: 2000
            });
        }
    }
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

// Global function to be called after AJAX updates
window.loadRestaurantsFromList = loadRestaurantsFromList;
window.centerMapOnRestaurant = centerMapOnRestaurant;

