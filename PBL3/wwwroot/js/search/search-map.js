// Search Map Functionality
let map;
let markers = [];
let currentPopup = null;
let mapisDragging = false;
let mapMoved = false;
let markerSpider; // MarkerSpider instance
let clusterSource; // Mapbox cluster source
let unclustered = []; // Store unclustered markers

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing search map...');
    
    // Reliable script loading mechanism with proper error handling
    const loadMarkerSpiderScript = () => {
        return new Promise((resolve, reject) => {
            // Check if MarkerSpider is already defined globally
            if (typeof MarkerSpider !== 'undefined') {
                console.log('MarkerSpider already defined globally');
                resolve();
                return;
            }
            
            // Add script dynamically if not already loaded
            const existingScript = document.querySelector('script[src*="marker-spider.js"]');
            if (!existingScript) {
                const script = document.createElement('script');
                script.type = 'text/javascript';
                script.src = '/js/search/marker-spider.js';
                
                // Properly handle script loading events
                script.onload = () => {
                    console.log('MarkerSpider script loaded successfully');
                    // Give a small timeout to ensure class definition is processed
                    setTimeout(() => {
                        if (typeof MarkerSpider !== 'undefined') {
                            console.log('MarkerSpider class is now available');
                            resolve();
                        } else {
                            console.error('MarkerSpider script loaded but class is not defined');
                            window.markerSpiderFailed = true;
                            reject(new Error('MarkerSpider not defined after script load'));
                        }
                    }, 300); // Increased timeout for slower connections
                };
                
                script.onerror = (e) => {
                    console.error('Error loading MarkerSpider script:', e);
                    window.markerSpiderFailed = true;
                    reject(new Error('Failed to load MarkerSpider script'));
                };
                
                document.head.appendChild(script);
            } else {
                console.log('MarkerSpider script already in DOM');
                // Script is in DOM but may not be loaded yet - give it more time
                let attempts = 0;
                const checkInterval = setInterval(() => {
                    attempts++;
                    if (typeof MarkerSpider !== 'undefined') {
                        clearInterval(checkInterval);
                        console.log('MarkerSpider found after waiting');
                        resolve();
                    } else if (attempts >= 20) { // Try for ~2 seconds (20*100ms)
                        clearInterval(checkInterval);
                        console.error('MarkerSpider not defined after multiple checks');
                        window.markerSpiderFailed = true;
                        reject(new Error('MarkerSpider not available after waiting'));
                    }
                }, 100);
            }
        });
    };
    
    // Load script then initialize map
    loadMarkerSpiderScript()
        .then(() => {
            console.log('MarkerSpider loaded, initializing map...');
            initializeMap();
        })
        .catch((error) => {
            console.error('Failed to load MarkerSpider, initializing map without clustering/spiderfy:', error);
            // Still initialize map but without MarkerSpider
            window.markerSpiderFailed = true;
            initializeMap();
        });
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
        });
        
        // Add navigation controls
        map.addControl(new mapboxgl.NavigationControl(), 'top-right');

    // Wait for map to load before adding markers
        map.on('load', function() {
            console.log('Map loaded successfully');
            
            // Initialize marker spider for handling overlapping markers if available
            if (typeof MarkerSpider !== 'undefined' && !window.markerSpiderFailed) {
                try {
                    console.log('Creating MarkerSpider instance with map:', map);
                    markerSpider = new MarkerSpider(map, {
                        circleSpiralSwitchover: 9,
                        circleFootSeparation: 40,
                        spiralFootSeparation: 30,
                        spiralLengthStart: 20,
                        spiralLengthFactor: 4,
                        animate: true,
                        animationDuration: 200
                    });
                    console.log('MarkerSpider initialized successfully');
                } catch (error) {
                    console.error('Error initializing MarkerSpider:', error);
                    window.markerSpiderFailed = true;
                    markerSpider = null;
                }
            } else {
                console.warn('MarkerSpider not available, skipping spiderfy functionality');
                window.markerSpiderFailed = true;
                markerSpider = null;
            }
            
            // Setup clustering layers and event handlers
            setupMapClusterLayers();
            
            // Now load restaurants
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
    
    // Make sure we have the cluster source
    if (!clusterSource && map.getSource('restaurants')) {
        clusterSource = map.getSource('restaurants');
    }
    
    if (!clusterSource) {
        console.warn('Cluster source not initialized yet');
        setupMapClusterLayers();
        clusterSource = map.getSource('restaurants');
    }

    // Get restaurant cards from the page
    const restaurantCards = document.querySelectorAll('.restaurant-item');
    console.log('Found restaurant cards:', restaurantCards.length);

    const bounds = new mapboxgl.LngLatBounds();
    const features = [];

    restaurantCards.forEach((card, index) => {
        const lat = parseFloat(card.dataset.lat);
        const lng = parseFloat(card.dataset.lng);
        const name = card.dataset.name || card.querySelector('.card-title')?.textContent || 'Unknown Restaurant';
        const rating = card.dataset.rating || card.querySelector('.fw-bold')?.nextElementSibling?.textContent || 'N/A';
        const reviewCount = card.dataset.reviewCount || card.querySelector('.text-muted.small')?.textContent.match(/\((\d+) reviews\)/)?.[1] || '0';
        const address = card.dataset.address || card.querySelector('.bi-geo-alt')?.parentNode?.textContent.trim() || '';
        const id = card.dataset.id;

        if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
            console.log(`Adding restaurant to GeoJSON: ${name} at (${lat}, ${lng})`);
            
            // Create GeoJSON feature
            const feature = {
                type: 'Feature',
                properties: {
                    id: id,
                    name: name,
                    rating: rating,
                    reviewCount: reviewCount,
                    address: address,
                    index: index + 1
                },
                geometry: {
                    type: 'Point',
                    coordinates: [lng, lat]
                }
            };
            
            features.push(feature);
            bounds.extend([lng, lat]);
        } else {
            console.warn(`Invalid coordinates for restaurant: ${name}, lat: ${lat}, lng: ${lng}`);
        }
    });

    console.log(`Added ${features.length} restaurants to GeoJSON`);

    // Update the GeoJSON source with our features
    if (clusterSource) {
        clusterSource.setData({
            type: 'FeatureCollection',
            features: features
        });
    }

    // Fit map to show all markers if any were added
    if (features.length > 0) {
        try {
            map.fitBounds(bounds, {
                padding: { top: 50, bottom: 50, left: 50, right: 50 },
                maxZoom: 15
            });
        } catch (error) {
            console.warn('Error fitting bounds:', error);
        }
    }
    
    // Handle non-clustered markers - if any features are not being clustered
    // This usually happens at higher zoom levels
    checkAndHandleOverlappingMarkers();
    
    // Listen for the zoom end event to check for overlapping markers
    map.on('zoomend', checkAndHandleOverlappingMarkers);
}

// Check for and handle overlapping markers that aren't being clustered
function checkAndHandleOverlappingMarkers() {
    if (!map) return;
    
    // If MarkerSpider is not available but we're at high zoom, still try to show markers
    if (!markerSpider && map.getZoom() > 14) {
        console.warn('MarkerSpider not available, showing regular markers instead');
        return;
    }
    
    // Only perform spiderfying at high zoom levels when clustering is no longer active
    if (map.getZoom() <= 14) return;
    
    // Get all visible unclustered points
    const unclusteredFeatures = map.queryRenderedFeatures({ layers: ['restaurant-points'] });
    
    if (!unclusteredFeatures.length) return;
    
    // Create a spatial index to find points near each other
    const pointGroups = {};
    
    unclusteredFeatures.forEach(feature => {
        const key = feature.geometry.coordinates.join(',');
        if (!pointGroups[key]) {
            pointGroups[key] = [];
        }
        pointGroups[key].push(feature);
    });
    
    // Find groups with more than one marker at the same location
    Object.values(pointGroups).forEach(group => {
        if (group.length > 1) {
            console.log(`Found ${group.length} markers at the same location`);
            handleOverlappingMarkers(group);
        }
    });
}

// Handle overlapping markers by using spiderfy
function handleOverlappingMarkers(features) {
    // Safety check - if MarkerSpider is not available, show simple popups instead
    if (!markerSpider || window.markerSpiderFailed) {
        console.warn('MarkerSpider not available, showing regular popup instead');
        if (features && features.length > 0) {
            const feature = features[0]; // Just show the first one
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            // Create and show popup
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({ closeButton: false })
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(map);
                
            if (currentPopup) currentPopup.remove();
            currentPopup = popup;
        }
        return;
    }
    
    try {
        // Clear any previous markers
        markerSpider.clearMarkers();
        
        // Convert features to markers
        const markerGroup = [];
        
        features.forEach((feature, index) => {
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            // Create marker element
            const markerElement = createMarkerElement(index + 1, rating || 'N/A');
            
            // Create marker
            const marker = new mapboxgl.Marker({
                element: markerElement,
                anchor: 'center'
            })
            .setLngLat(coordinates)
            .addTo(map);
            
            // Create popup for this marker
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({
                closeButton: false,
                closeOnClick: false,
            }).setHTML(popupContent);
            
            // Setup hover events
            setupMarkerHoverEvents(markerElement, marker, popup);
            
            // Add to tracking arrays
            unclustered.push({ marker, popup, id });
            
            // Add to MarkerSpider
            markerSpider.addMarker(marker, id);
            
            // Track pixel position for spiderfying
            markerGroup.push({
                id,
                marker,
                pixelPos: map.project(coordinates)
            });
        });
        
        // Spiderfy the markers
        if (markerGroup.length > 1) {
            markerSpider.spiderfy(markerGroup);
        }
    } catch (error) {
        console.error('Error in handleOverlappingMarkers:', error);
        // Fallback to showing a single popup if spiderfying fails
        if (features && features.length > 0) {
            const feature = features[0];
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({ closeButton: false })
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(map);
                
            if (currentPopup) currentPopup.remove();
            currentPopup = popup;
        }
    }
}

// Setup hover events for a marker
function setupMarkerHoverEvents(markerElement, marker, popup) {
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
    });
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
    // Clear regular markers
    markers.forEach(({ marker, popup }) => {
        if (popup) popup.remove();
        if (marker) marker.remove();
    });
    markers = [];
    
    // Clear unclustered markers
    unclustered.forEach(({ marker, popup }) => {
        if (popup) popup.remove();
        if (marker) marker.remove();
    });
    unclustered = [];
    
    // Clear spider markers
    if (markerSpider) {
        try {
            markerSpider.clearMarkers();
        } catch (error) {
            console.error('Error clearing spider markers:', error);
        }
    }
    
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
        
        // Move map to restaurant location
        map.flyTo({
            center: [lng, lat],
            zoom: zoomLevel,
            duration: duration
        });
        
        // Wait for map movement to complete
        setTimeout(() => {
            // After zooming in, check for markers at this location
            const features = map.queryRenderedFeatures(
                map.project([lng, lat]),
                { layers: ['restaurant-points'] }
            );
            
            // Find features at this location with matching ID
            const matchingFeatures = features.filter(feature => 
                feature.properties.id.toString() === strId
            );
            
            // If we have multiple features at this location, spiderfy them
            if (features.length > 1) {
                console.log(`Found ${features.length} overlapping markers at location, spiderfying`);
                handleOverlappingMarkers(features);
            } 
            
            // Show popup for this restaurant
            const popupContent = createPopupContent(
                card.dataset.name || card.querySelector('.card-title')?.textContent || 'Unknown Restaurant',
                card.dataset.rating || 'N/A',
                card.dataset.reviewCount || '0',
                card.dataset.address || '',
                strId
            );
            
            // Close any existing popup
            if (currentPopup) {
                currentPopup.remove();
            }
            
            // Create and show new popup
            const popup = new mapboxgl.Popup({
                closeButton: false,
                closeOnClick: false,
            })
            .setLngLat([lng, lat])
            .setHTML(popupContent)
            .addTo(map);
            
            currentPopup = popup;
            
        }, duration + 100); // Wait a bit after flyTo completes
        
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

// Setup map clustering layers
function setupMapClusterLayers() {
    // Check if source already exists and remove it
    if (map.getSource('restaurants')) {
        if (map.getLayer('restaurant-clusters')) map.removeLayer('restaurant-clusters');
        if (map.getLayer('cluster-count')) map.removeLayer('cluster-count');
        if (map.getLayer('restaurant-points')) map.removeLayer('restaurant-points');
        map.removeSource('restaurants');
    }
    
    // Add a new source with empty GeoJSON data (will be filled in loadRestaurantsFromList)
    map.addSource('restaurants', {
        type: 'geojson',
        data: {
            type: 'FeatureCollection',
            features: []
        },
        cluster: true,
        clusterMaxZoom: 14, // Max zoom to cluster points on
        clusterRadius: 50 // Radius of each cluster when clustering points
    });
    
    // Save reference to the source for later updates
    clusterSource = map.getSource('restaurants');

    // Add a layer showing the clusters
    map.addLayer({
        id: 'restaurant-clusters',
        type: 'circle',
        source: 'restaurants',
        filter: ['has', 'point_count'],
        paint: {
            // Use step expressions for circles with different sizes/colors based on point count
            'circle-color': [
                'step',
                ['get', 'point_count'],
                '#51bbd6', // Color for small clusters
                10, '#f1f075', // Color for medium clusters
                25, '#f28cb1'  // Color for large clusters
            ],
            'circle-radius': [
                'step',
                ['get', 'point_count'],
                20, // Size for small clusters
                10, 25, // Size for medium clusters
                25, 30  // Size for large clusters
            ],
            'circle-stroke-width': 1,
            'circle-stroke-color': '#fff'
        }
    });

    // Add a layer for the cluster count text
    map.addLayer({
        id: 'cluster-count',
        type: 'symbol',
        source: 'restaurants',
        filter: ['has', 'point_count'],
        layout: {
            'text-field': '{point_count_abbreviated}',
            'text-font': ['DIN Offc Pro Medium', 'Arial Unicode MS Bold'],
            'text-size': 12
        },
        paint: {
            'text-color': '#ffffff'
        }
    });

    // Setup cluster click event - zoom in on click
    map.on('click', 'restaurant-clusters', function(e) {
        const features = map.queryRenderedFeatures(e.point, {
            layers: ['restaurant-clusters']
        });
        
        const clusterId = features[0].properties.cluster_id;
        const pointCount = features[0].properties.point_count;
        const clusterSource = map.getSource('restaurants');
        
        console.log(`Cluster clicked: ID ${clusterId}, contains ${pointCount} points`);
        
        // If we're zoomed in enough and there are many points, expand with spiderfy instead of zooming
        if (map.getZoom() > 14 && pointCount > 2) {
            clusterSource.getClusterLeaves(clusterId, pointCount, 0, function(error, features) {
                if (error) {
                    console.error('Error getting cluster leaves:', error);
                    return;
                }
                
                // Handle overlapping markers with spiderfy
                handleOverlappingMarkers(features);
            });
        } else {
            // Otherwise zoom in
            clusterSource.getClusterExpansionZoom(clusterId, function(error, zoom) {
                if (error) return;
                
                map.easeTo({
                    center: features[0].geometry.coordinates,
                    zoom: zoom + 0.5 // Add a bit more zoom
                });
            });
        }
    });

    // Change cursor on cluster hover
    map.on('mouseenter', 'restaurant-clusters', function() {
        map.getCanvas().style.cursor = 'pointer';
    });
    
    map.on('mouseleave', 'restaurant-clusters', function() {
        map.getCanvas().style.cursor = '';
    });

    // Add a layer for unclustered points
    map.addLayer({
        id: 'restaurant-points',
        type: 'circle',
        source: 'restaurants',
        filter: ['!', ['has', 'point_count']],
        paint: {
            'circle-color': '#dc3545', // Red color for individual markers
            'circle-radius': 10,
            'circle-stroke-width': 1,
            'circle-stroke-color': '#fff'
        }
    });
    
    // Set up unclustered point click
    map.on('click', 'restaurant-points', function(e) {
        const features = map.queryRenderedFeatures(e.point, {
            layers: ['restaurant-points']
        });
        
        if (!features.length) return;
        
        // Get the list of features at this location (there might be multiple points at the same location)
        const overlappingFeatures = map.queryRenderedFeatures(e.point, { layers: ['restaurant-points'] });
        
        if (overlappingFeatures.length > 1) {
            // If there are multiple points at the same location, spiderfy them
            console.log(`Found ${overlappingFeatures.length} overlapping markers at this point`);
            handleOverlappingMarkers(overlappingFeatures);
        } else {
            // If only one point, show its popup
            const feature = features[0];
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            // Create popup content
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            // Create and show popup
            const popup = new mapboxgl.Popup({ closeButton: false })
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(map);
                
            // Store as current popup
            if (currentPopup) currentPopup.remove();
            currentPopup = popup;
        }
    });
    
    // Change cursor when hovering over a point
    map.on('mouseenter', 'restaurant-points', function() {
        map.getCanvas().style.cursor = 'pointer';
    });
    
    map.on('mouseleave', 'restaurant-points', function() {
        map.getCanvas().style.cursor = '';
    });
}

// Handle overlapping markers by using spiderfy
function handleOverlappingMarkers(features) {
    // Safety check - if MarkerSpider is not available, show simple popups instead
    if (!markerSpider || window.markerSpiderFailed) {
        console.warn('MarkerSpider not available, showing regular popup instead');
        if (features && features.length > 0) {
            const feature = features[0]; // Just show the first one
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            // Create and show popup
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({ closeButton: false })
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(map);
                
            if (currentPopup) currentPopup.remove();
            currentPopup = popup;
        }
        return;
    }
    
    try {
        // Clear any previous markers
        markerSpider.clearMarkers();
        
        // Convert features to markers
        const markerGroup = [];
        
        features.forEach((feature, index) => {
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            // Create marker element
            const markerElement = createMarkerElement(index + 1, rating || 'N/A');
            
            // Create marker
            const marker = new mapboxgl.Marker({
                element: markerElement,
                anchor: 'center'
            })
            .setLngLat(coordinates)
            .addTo(map);
            
            // Create popup for this marker
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({
                closeButton: false,
                closeOnClick: false,
            }).setHTML(popupContent);
            
            // Setup hover events
            setupMarkerHoverEvents(markerElement, marker, popup);
            
            // Add to tracking arrays
            unclustered.push({ marker, popup, id });
            
            // Add to MarkerSpider
            markerSpider.addMarker(marker, id);
            
            // Track pixel position for spiderfying
            markerGroup.push({
                id,
                marker,
                pixelPos: map.project(coordinates)
            });
        });
        
        // Spiderfy the markers
        if (markerGroup.length > 1) {
            markerSpider.spiderfy(markerGroup);
        }
    } catch (error) {
        console.error('Error in handleOverlappingMarkers:', error);
        // Fallback to showing a single popup if spiderfying fails
        if (features && features.length > 0) {
            const feature = features[0];
            const coordinates = feature.geometry.coordinates.slice();
            const { id, name, rating, reviewCount, address } = feature.properties;
            
            const popupContent = createPopupContent(
                name || 'Unknown Restaurant',
                rating || 'N/A',
                reviewCount || '0',
                address || '',
                id
            );
            
            const popup = new mapboxgl.Popup({ closeButton: false })
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(map);
                
            if (currentPopup) currentPopup.remove();
            currentPopup = popup;
        }
    }
}

