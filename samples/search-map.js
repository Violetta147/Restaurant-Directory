// Mapbox initialization and restaurant map functionality
document.addEventListener('DOMContentLoaded', () => {
    console.log('search-map.js loaded');
    
    // Initialize variables
    let map;
    let restaurantMarkers = [];
    let mapIsDragging = false;
    let mapMoved = false;

    // Get initial data
    const initialLatElement = document.getElementById('map-initial-lat');
    const initialLngElement = document.getElementById('map-initial-lng');
    const initialQueryElement = document.getElementById('map-initial-query');
    const initialCategoryElement = document.getElementById('map-initial-category');

    console.log('Initial elements found:', {
        lat: !!initialLatElement,
        lng: !!initialLngElement,
        query: !!initialQueryElement,
        category: !!initialCategoryElement
    });

    if (!initialLatElement || !initialLngElement) {
        console.error('Required map initialization elements not found');
        return;
    }

    const initialLat = parseFloat(initialLatElement.value);
    const initialLng = parseFloat(initialLngElement.value);
    const initialQuery = initialQueryElement ? initialQueryElement.value : '';
    const initialCategory = initialCategoryElement ? initialCategoryElement.value : '';

    console.log('Initial values:', { initialLat, initialLng, initialQuery, initialCategory });

    // Initialize map
    initializeMap();
    setupEventListeners();

    // Handle window resize to ensure map takes full available space
    window.addEventListener('resize', () => {
        if (map) {
            setTimeout(() => {
                map.resize();
            }, 100);
        }
    });

    function initializeMap() {
        try {
            // Check if Mapbox is loaded
            if (typeof mapboxgl === 'undefined') {
                console.error('Mapbox GL JS is not loaded');
                return;
            }

            // Get Mapbox token
            const tokenElement = document.getElementById('mapbox-token');
            if (!tokenElement || !tokenElement.value) {
                console.error('Mapbox token not found');
                return;
            }

            // Initialize the map
            mapboxgl.accessToken = tokenElement.value;

            const mapContainer = document.getElementById('mapbox-container');
            if (!mapContainer) {
                console.error('Map container not found');
                return;
            }

            console.log('Initializing map with center:', [initialLng, initialLat]);

            // Log container dimensions for debugging
            const rect = mapContainer.getBoundingClientRect();
            console.log('Map container dimensions:', rect.width + 'x' + rect.height);
            console.log('Map container computed styles:', {
                width: window.getComputedStyle(mapContainer).width,
                height: window.getComputedStyle(mapContainer).height,
                display: window.getComputedStyle(mapContainer).display,
                visibility: window.getComputedStyle(mapContainer).visibility
            });

            map = new mapboxgl.Map({
                container: 'mapbox-container',
                style: 'mapbox://styles/mapbox/streets-v12',
                center: [initialLng, initialLat],
                zoom: 13
            });

            // Add navigation controls
            map.addControl(new mapboxgl.NavigationControl());

            // Initialize map
            map.on('load', function () {
                console.log('Map loaded successfully');
                // Force map to resize to fit container
                setTimeout(() => {
                    map.resize();
                }, 100);

                // Add user location control with high accuracy
                map.addControl(new mapboxgl.GeolocateControl({
                    positionOptions: {
                        enableHighAccuracy: true
                    },
                    trackUserLocation: true
                }));

                // Add restaurants from the model to the map
                loadRestaurantsFromList();
            });

            map.on('error', function(e) {
                console.error('Map error:', e);
            });

            // Add this to test if map instance is created
            console.log('Map instance created:', !!map);
            
        } catch (error) {
            console.error('Error initializing map:', error);
        }

        // Setup map move events
        map.on('movestart', function () {
            mapIsDragging = true;
        });

        map.on('moveend', function () {
            mapIsDragging = false;
            if (mapMoved) {
                document.getElementById('reload-map-button').style.display = 'block';
            }
            mapMoved = true;
        });

        map.on('move', function () {
            if (mapIsDragging) {
                mapMoved = true;
            }
        });
    }

    function loadRestaurantsFromList() {
        try {
            console.log('Loading restaurants from list...');
            
            // Clear existing markers
            clearMarkers();

            // đi tìm trong trang các thẻ có class là restaurant-item tức là nhà hàng
            // rồi lấy thông tin cần thiết từ các thẻ để tạo marker
            const restaurantItems = document.querySelectorAll('.restaurant-item');
            console.log('Found restaurant items:', restaurantItems.length);

            restaurantItems.forEach(item => {
                const id = parseInt(item.dataset.id);
                const lat = parseFloat(item.dataset.lat);
                const lng = parseFloat(item.dataset.lng);
                const slug = item.dataset.slug || '';
                const nameElement = item.querySelector('h5.card-title') || item.querySelector('h3') || item.querySelector('.card-title');
                const name = nameElement ? nameElement.textContent.trim() : 'Unknown Restaurant';

                if (!isNaN(lat) && !isNaN(lng)) {
                    console.log('Adding marker for:', name, lat, lng, 'slug:', slug);
                    // Create restaurant marker
                    addRestaurantMarker(id, lat, lng, name, '', '', slug);
                } else {
                    console.warn('Invalid coordinates for restaurant:', name, lat, lng);
                }
            });

            console.log('Total markers added:', restaurantMarkers.length);

            // Fit map to show all markers if we have any
            if (restaurantMarkers.length > 0) {
                fitMapToMarkers();
            }
        } catch (error) {
            console.error('Error loading restaurants from list:', error);
        }
    }

    function addRestaurantMarker(id, lat, lng, name, address = '', rating = '', slug = '') {
        // Create custom HTML element for the marker
        const el = document.createElement('div');
        el.className = 'restaurant-marker';
        el.innerHTML = `<i class="fas fa-utensils text-primary text-lg"></i>`;
        el.style.width = '30px';
        el.style.height = '30px';
        el.style.borderRadius = '50%';
        el.style.backgroundColor = 'white';
        el.style.display = 'flex';
        el.style.justifyContent = 'center';
        el.style.alignItems = 'center';
        el.style.boxShadow = '0 2px 4px rgba(0,0,0,0.3)';
        el.style.cursor = 'pointer';

        // Use slug for SEO-friendly URL or fallback to old format
        const detailUrl = slug ? `/restaurant/${slug}` : `/Restaurant/Details/${id}`;

        // Create popup
        const popup = new mapboxgl.Popup({ offset: 25 })
            .setHTML(`
                <div class="p-2">
                    <h3 class="font-bold text-lg">${name}</h3>
                    ${address ? `<p class="text-sm">${address}</p>` : ''}
                    ${rating ? `<div class="flex items-center mt-1">
                        <span class="text-yellow-500">★</span>
                        <span class="ml-1">${rating}</span>
                    </div>` : ''}
                    <a href="${detailUrl}" class="block mt-2 px-3 py-1 bg-primary text-white rounded text-sm text-center">
                        View Details
                    </a>
                </div>
            `);

        // Create marker
        const marker = new mapboxgl.Marker(el)
            .setLngLat([lng, lat])
            .setPopup(popup)
            .addTo(map);

        // Store marker for later reference
        restaurantMarkers.push({
            id: id,
            marker: marker,
            element: el
        });
    }

    function clearMarkers() {
        // Remove all existing markers
        restaurantMarkers.forEach(m => {
            m.marker.remove();
        });

        restaurantMarkers = [];
    }

    function fitMapToMarkers() {
        if (restaurantMarkers.length === 0) return;

        const bounds = new mapboxgl.LngLatBounds();

        restaurantMarkers.forEach(m => {
            bounds.extend(m.marker.getLngLat());
        });

        map.fitBounds(bounds, {
            padding: 50,
            maxZoom: 15
        });
    }

    function loadRestaurantsInMapArea() {
        // Get current map center and bounds
        const center = map.getCenter();
        const lat = center.lat;
        const lng = center.lng;

        // Make AJAX request to get restaurants in this area - Updated to use SearchController
        fetch(`/Search/GetRestaurantsJson?lat=${lat}&lng=${lng}&selectedDistanceCategory=3km&query=${encodeURIComponent(initialQuery)}&selectedCategory=${encodeURIComponent(initialCategory)}`)
            .then(response => response.json())
            .then(data => {
                // Clear existing markers
                clearMarkers();

                // Add new markers from GeoJSON
                if (data.features && data.features.length > 0) {
                    data.features.forEach(feature => {
                        const id = feature.properties.id;
                        const lat = feature.geometry.coordinates[1];
                        const lng = feature.geometry.coordinates[0];
                        const name = feature.properties.name;
                        const address = feature.properties.address || '';
                        const rating = feature.properties.rating || '';
                        const slug = feature.properties.slug || id; // Use slug for URL

                        // Add marker
                        addRestaurantMarker(id, lat, lng, name, address, rating, slug);
                    });
                }

                // Hide reload button if it exists
                const reloadButton = document.getElementById('reload-map-button');
                if (reloadButton) {
                    reloadButton.style.display = 'none';
                }

                // Reset map moved flag
                mapMoved = false;
            })
            .catch(error => {
                console.error('Error loading restaurants:', error);
            });
    }

    function setupEventListeners() {
        // "Reload Map" button click
        const reloadButton = document.getElementById('reload-map-button');
        if (reloadButton) {
            reloadButton.addEventListener('click', loadRestaurantsInMapArea);
        }

        // Restaurant list item click - delegate event for dynamic content
        document.addEventListener('click', function(e) {
            if (e.target.closest('.restaurant-item')) {
                const item = e.target.closest('.restaurant-item');
                const id = parseInt(item.dataset.id);
                const lat = parseFloat(item.dataset.lat);
                const lng = parseFloat(item.dataset.lng);
                
                // Find and open popup
                const markerObj = restaurantMarkers.find(m => m.id === id);
                if (markerObj) {
                    markerObj.marker.togglePopup();
                }
            }
        });

        // Apply filters button - REMOVED CONFLICTING JAVASCRIPT
        // The HTML form now handles filter submission naturally via type="submit"
        // No JavaScript intervention needed - ASP.NET model binding handles everything
        
        // Price filter toggle - REMOVED (not used in actual HTML form)
    }

    // Expose loadRestaurantsFromList globally for AJAX callback
    window.loadRestaurantsFromList = loadRestaurantsFromList;
});