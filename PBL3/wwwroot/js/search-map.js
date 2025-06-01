// Mapbox initialization and restaurant map functionality
document.addEventListener('DOMContentLoaded', () => {
    // Initialize variables
    let map;
    let restaurantMarkers = [];
    let mapIsDragging = false;
    let mapMoved = false;

    // Get initial data
    const initialLat = parseFloat(document.getElementById('map-initial-lat').value);
    const initialLng = parseFloat(document.getElementById('map-initial-lng').value);
    const initialQuery = document.getElementById('map-initial-query').value;
    const initialCategory = document.getElementById('map-initial-category').value;

    // Initialize map
    initializeMap();
    setupEventListeners();

    function initializeMap() {
        // Initialize the map
        mapboxgl.accessToken = document.getElementById('mapbox-token').value;

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
        // Clear existing markers
        clearMarkers();

        // Get all restaurant items from the list and add markers
        const restaurantItems = document.querySelectorAll('.restaurant-item');

        restaurantItems.forEach(item => {
            const id = parseInt(item.dataset.id);
            const lat = parseFloat(item.dataset.lat);
            const lng = parseFloat(item.dataset.lng);
            const name = item.querySelector('h3').textContent;

            // Create restaurant marker
            addRestaurantMarker(id, lat, lng, name);
        });

        // Fit map to show all markers if we have any
        if (restaurantMarkers.length > 0) {
            fitMapToMarkers();
        }
    }

    function addRestaurantMarker(id, lat, lng, name, address = '', rating = '') {
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
                    <a href="/Restaurant/Details/${id}" class="block mt-2 px-3 py-1 bg-primary text-white rounded text-sm text-center">
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

        // Click event to highlight corresponding list item
        el.addEventListener('click', () => {
            highlightRestaurantInList(id);
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

        // Make AJAX request to get restaurants in this area
        fetch(`/Map/GetRestaurantsJson?lat=${lat}&lng=${lng}&radius=3.0&q=${encodeURIComponent(initialQuery)}&category=${encodeURIComponent(initialCategory)}`)
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

                        // Add marker
                        addRestaurantMarker(id, lat, lng, name, address, rating);
                    });
                }

                // Hide reload button
                document.getElementById('reload-map-button').style.display = 'none';

                // Reset map moved flag
                mapMoved = false;
            })
            .catch(error => {
                console.error('Error loading restaurants:', error);
            });
    }

    function highlightRestaurantInList(id) {
        // Remove highlight from all items
        document.querySelectorAll('.restaurant-item').forEach(item => {
            item.style.border = 'none';
        });

        // Find and highlight item with matching id
        const item = document.querySelector(`.restaurant-item[data-id="${id}"]`);
        if (item) {
            item.style.border = '2px solid #FF6E40';
            item.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    function highlightMarkerById(id) {
        // Reset all markers
        restaurantMarkers.forEach(m => {
            m.element.style.backgroundColor = 'white';
            m.element.querySelector('i').style.color = '#FF6E40';  // primary color
        });

        // Find and highlight the marker
        const markerObj = restaurantMarkers.find(m => m.id === id);
        if (markerObj) {
            markerObj.element.style.backgroundColor = '#FF6E40';
            markerObj.element.querySelector('i').style.color = 'white';
        }
    }

    function setupEventListeners() {
        // "Reload Map" button click
        document.getElementById('reload-map-button').addEventListener('click', loadRestaurantsInMapArea);

        // Restaurant list item click
        document.querySelectorAll('.restaurant-item').forEach(item => {
            item.addEventListener('click', function () {
                const id = parseInt(this.dataset.id);
                const lat = parseFloat(this.dataset.lat);
                const lng = parseFloat(this.dataset.lng);

                // Fly to this restaurant on the map
                map.flyTo({
                    center: [lng, lat],
                    zoom: 16
                });

                // Highlight this item
                highlightRestaurantInList(id);

                // Highlight marker
                highlightMarkerById(id);

                // Find and open popup
                const markerObj = restaurantMarkers.find(m => m.id === id);
                if (markerObj) {
                    markerObj.marker.togglePopup();
                }
            });
        });

        // "View on Map" button click
        document.querySelectorAll('.view-on-map-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation(); // Prevent parent click

                const item = this.closest('.restaurant-item');
                const id = parseInt(item.dataset.id);
                const lat = parseFloat(item.dataset.lat);
                const lng = parseFloat(item.dataset.lng);

                // Fly to this restaurant on the map
                map.flyTo({
                    center: [lng, lat],
                    zoom: 16
                });

                // Highlight this item
                highlightRestaurantInList(id);

                // Highlight marker
                highlightMarkerById(id);

                // Find and open popup
                const markerObj = restaurantMarkers.find(m => m.id === id);
                if (markerObj) {
                    markerObj.marker.togglePopup();
                }
            });
        });

        // Apply filters button
        document.getElementById('apply-filters-btn').addEventListener('click', function () {
            // Get selected filters
            const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
                .map(cb => cb.value).join(',');

            const selectedRating = document.querySelector('.rating-radio:checked')?.value || '';

            const selectedPrices = Array.from(document.querySelectorAll('.price-filter.active'))
                .map(btn => btn.dataset.price).join(',');

            // Build URL with filters
            let url = '/Search/Index?';

            if (initialQuery) {
                url += `q=${encodeURIComponent(initialQuery)}&`;
            }

            if (document.querySelector('.location-input')?.value) {
                url += `location=${encodeURIComponent(document.querySelector('.location-input').value)}&`;
            }

            if (selectedCategories) {
                url += `category=${encodeURIComponent(selectedCategories)}&`;
            }

            if (selectedRating) {
                url += `rating=${encodeURIComponent(selectedRating)}&`;
            }

            if (selectedPrices) {
                url += `price=${encodeURIComponent(selectedPrices)}&`;
            }

            // Redirect to filtered results
            window.location.href = url.endsWith('&')
                ? url.slice(0, -1) // Remove trailing &
                : url;
        });

        // Price filter toggle
        document.querySelectorAll('.price-filter').forEach(btn => {
            btn.addEventListener('click', function () {
                this.classList.toggle('active');
                if (this.classList.contains('active')) {
                    this.style.backgroundColor = '#FF6E40';
                    this.style.color = 'white';
                } else {
                    this.style.backgroundColor = 'white';
                    this.style.color = 'inherit';
                }
            });
        });
    }
});