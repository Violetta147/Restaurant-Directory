// Search Map Functionality with Clustering and Spiderifier
let map;
let markers = [];
let currentPopup = null;
let mapisDragging = false;
let mapMoved = false;
let spiderifier = null; // Spiderifier instance
let currentZoomLevel = 0; // Track current zoom level

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing search map with clustering and spiderifier...');
    initializeMap();
});

function initializeMap() {
    // Get map configuration from hidden inputs
    const mapboxToken = document.getElementById('mapbox-token')?.value;
    const initialLat = parseFloat(document.getElementById('map-initial-lat')?.value || '16.047079');
    console.log('Initial latitude:', isNaN(initialLat) ? 'Invalid' : initialLat);
    const initialLng = parseFloat(document.getElementById('map-initial-lng')?.value || '108.206230');
    console.log('Initial longitude:', isNaN(initialLng) ? 'Invalid' : initialLng);

    if (!mapboxToken) {
        console.warn('Mapbox token not found. Map will not be initialized.');
        showMapError('Map configuration missing');
        return;
    }

    console.log('Map config:', { lat: initialLat, lng: initialLng });

    try {
        // Set Mapbox access token
        mapboxgl.accessToken = mapboxToken;
        console.log('Mapbox access token set:', mapboxgl.accessToken);        // Initialize map
        map = new mapboxgl.Map({
            container: 'mapbox-container',
            style: 'mapbox://styles/mapbox/streets-v12',
            center: [initialLng, initialLat],
            zoom: 12,
            language: 'vi'
        });

        // Add navigation controls
        map.addControl(new mapboxgl.NavigationControl(), 'top-right');

        // Store initial zoom level
        currentZoomLevel = map.getZoom();
        console.log('Initial zoom level:', currentZoomLevel);

        // Add event listener for zoom changes
        map.on('zoom', function () {
            const newZoomLevel = map.getZoom();
            console.log('Current zoom level:', newZoomLevel);
            currentZoomLevel = newZoomLevel;
        });        // Wait for map to load before adding markers
        map.on('load', function () {
            console.log('Map loaded successfully');
            console.log('Map zoom level at load:', map.getZoom());

            // Initialize spiderifier after map loads
            console.log('Initializing spiderifier...');
            try {
                // Check if MapboxglSpiderifier exists
                if (typeof MapboxglSpiderifier === 'undefined') {
                    console.error('MapboxglSpiderifier is not defined! Make sure the library is properly loaded.');
                    return;
                }

                console.log('MapboxglSpiderifier constructor found, initializing...');
                spiderifier = new MapboxglSpiderifier(map, {
                    animate: true,
                    animationSpeed: 300, // Slightly slower for better visibility
                    customPin: false,
                    radius: 100, // Further increased radius for better visibility
                    circleFootSeparation: 60, // Further increased separation
                    spiralFootSeparation: 45, // Further increased separation
                    spiralLengthStart: 30, // Start the spiral even further away
                    spiralLengthFactor: 5, // Increased factor for better spacing
                    initializeLeg: function (pin, feature) {
                        console.log('Initializing spiderifier leg for feature:', feature);

                        // Enhanced styling for pins
                        pin.style.backgroundColor = '#dc3545';
                        pin.style.border = '2px solid white';
                        pin.style.width = '36px';
                        pin.style.height = '36px';
                        pin.style.borderRadius = '50%';
                        pin.style.display = 'flex';
                        pin.style.alignItems = 'center';
                        pin.style.justifyContent = 'center';
                        pin.style.color = 'white';
                        pin.style.fontWeight = 'bold';
                        pin.style.fontSize = '14px';
                        pin.style.boxShadow = '0 3px 6px rgba(0,0,0,0.3)';
                        pin.style.transition = 'all 0.2s ease-in-out';

                        // Use different colors for different restaurant types if available
                        if (feature.properties && feature.properties.cuisineType) {
                            const cuisineType = feature.properties.cuisineType.toLowerCase();
                            if (cuisineType.includes('vietnamese')) {
                                pin.style.backgroundColor = '#d32f2f'; // Red
                            } else if (cuisineType.includes('italian')) {
                                pin.style.backgroundColor = '#388e3c'; // Green
                            } else if (cuisineType.includes('japanese')) {
                                pin.style.backgroundColor = '#1976d2'; // Blue
                            } else if (cuisineType.includes('chinese')) {
                                pin.style.backgroundColor = '#ffa000'; // Amber
                            }
                        }

                        // Display the number/index or another identifier
                        if (feature.properties && feature.properties.index) {
                            pin.textContent = feature.properties.index;
                        } else if (feature.properties && feature.properties.number) {
                            pin.textContent = feature.properties.number;
                        } else if (feature.properties && feature.properties.name) {
                            // If no index, try first letter of name
                            pin.textContent = feature.properties.name.charAt(0).toUpperCase();
                        } else {
                            // Fallback
                            pin.textContent = "#";
                        }

                        // Add hover effect event listeners
                        pin.addEventListener('mouseenter', function () {
                            pin.style.transform = 'scale(1.1)';
                            pin.style.boxShadow = '0 4px 8px rgba(0,0,0,0.4)';
                        });

                        pin.addEventListener('mouseleave', function () {
                            pin.style.transform = 'scale(1)';
                            pin.style.boxShadow = '0 3px 6px rgba(0,0,0,0.3)';
                        });

                        console.log('Leg pin styled:', pin.style.cssText);
                    }, onClick: function (e, feature) {
                        e.stopPropagation(); // Prevent event bubbling

                        if (currentPopup) currentPopup.remove();

                        const markerCoords = feature.geometry.coordinates;

                        const popup = new mapboxgl.Popup({
                            closeButton: true,
                            closeOnClick: false,
                            className: 'spider-popup',
                            maxWidth: '300px',
                            offset: [0, -18],
                            anchor: 'bottom'
                        }).setHTML(feature.properties.popupContent);

                        popup.setLngLat(markerCoords);
                        popup.addTo(map);
                        currentPopup = popup;
                    }

                });

                console.log('Spiderifier initialized successfully');
            } catch (error) {
                console.error('Error initializing spiderifier:', error);
                spiderifier = null;
            }

            // Load restaurants after initializing the spiderifier
            loadRestaurantsFromList();
        });

        // Hide POI labels when style loads
        map.on('style.load', () => {
            map.getStyle().layers.forEach(function (layer) {
                if (layer.id.includes('poi') || layer.id.includes('poi-label')) {
                    map.setLayoutProperty(layer.id, 'visibility', 'none');
                }
            });
            console.log('POI labels hidden');
        });

        map.on('error', function (e) {
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

    // Clear any spiderifier markers
    if (spiderifier) {
        spiderifier.unspiderfy();
    }

    // Get restaurant cards from the page
    const restaurantCards = document.querySelectorAll('.restaurant-item');
    console.log('Found restaurant cards:', restaurantCards.length);

    const bounds = new mapboxgl.LngLatBounds();
    let markersAdded = 0;

    // Create an array to store GeoJSON features for clustering
    const features = [];

    // Check if we already have the cluster source and layer
    const hasClusterSource = map.getSource('restaurants');
    if (hasClusterSource) {
        // Remove existing layers and source if they exist
        if (map.getLayer('clusters')) map.removeLayer('clusters');
        if (map.getLayer('cluster-count')) map.removeLayer('cluster-count');
        if (map.getLayer('unclustered-point')) map.removeLayer('unclustered-point');
        if (map.getLayer('unclustered-point-label')) map.removeLayer('unclustered-point-label');
        map.removeSource('restaurants');
        console.log('Removed existing cluster layers and source');
    }

    // Process restaurant cards and create features for clustering
    restaurantCards.forEach((card, index) => {
        const lat = parseFloat(card.dataset.lat);
        const lng = parseFloat(card.dataset.lng);
        const name = card.dataset.name || card.querySelector('.card-title')?.textContent || 'Unknown Restaurant';
        const rating = card.dataset.rating || card.querySelector('.fw-bold')?.nextElementSibling?.textContent || 'N/A';
        const reviewCount = card.dataset.reviewCount || card.querySelector('.text-muted.small')?.textContent.match(/\((\d+) reviews\)/)?.[1] || '0';
        const address = card.dataset.address || card.querySelector('.bi-geo-alt')?.parentNode?.textContent.trim() || '';
        const id = card.dataset.id;

        if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
            console.log(`Adding feature for: ${name} at (${lat}, ${lng})`);

            // Create popup content
            const popupContent = createPopupContent(name, rating, reviewCount, address, id);

            // Add feature to the features array for clustering
            features.push({
                type: 'Feature',
                properties: {
                    id: id.toString(),
                    name: name,
                    rating: rating,
                    reviewCount: reviewCount,
                    address: address,
                    index: index + 1,
                    popupContent: popupContent,
                    number: (index + 1).toString()
                },
                geometry: {
                    type: 'Point',
                    coordinates: [lng, lat]
                }
            });

            bounds.extend([lng, lat]);
            markersAdded++;
        }
    });

    console.log(`Added ${markersAdded} features to map`);

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

        // Add cluster layers if features exist
        if (features.length > 0) {
            console.log('Setting up clustering with features:', features.length);

            // Add a new source with clustering enabled
            map.addSource('restaurants', {
                type: 'geojson',
                data: {
                    type: 'FeatureCollection',
                    features: features
                },
                cluster: true,
                clusterMaxZoom: 14, // Max zoom to cluster points on
                clusterRadius: 50 // Radius of each cluster when clustering points
            });

            // Add cluster circles layer
            map.addLayer({
                id: 'clusters',
                type: 'circle',
                source: 'restaurants',
                filter: ['has', 'point_count'],
                paint: {
                    'circle-color': [
                        'step',
                        ['get', 'point_count'],
                        '#dc3545', // Red for small clusters
                        10, // Step threshold
                        '#ff9800', // Orange for medium clusters 
                        30, // Step threshold
                        '#3f51b5' // Blue for large clusters
                    ],
                    'circle-radius': [
                        'step',
                        ['get', 'point_count'],
                        20, // Size for small clusters
                        10, // Step threshold
                        25, // Size for medium clusters
                        30, // Step threshold
                        30 // Size for large clusters
                    ],
                    'circle-stroke-width': 1,
                    'circle-stroke-color': '#fff'
                }
            });

            // Add count labels
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
            // Handle clicks on clusters - THE IMPORTANT CHANGE
            map.on('click', 'clusters', function (e) {
                const features = map.queryRenderedFeatures(e.point, { layers: ['clusters'] });
                if (!features || features.length === 0) {
                    console.warn('No cluster features found at click point');
                    return;
                } const clusterId = features[0].properties.cluster_id;
                const clusterCenter = features[0].geometry.coordinates;
                const pointCount = features[0].properties.point_count;

                console.log(`Cluster clicked: ID ${clusterId}, contains ${pointCount} points`);
                console.log(`Current zoom level when cluster clicked: ${map.getZoom()}`);
                console.log(`Tracked zoom level: ${currentZoomLevel}`);

                // Close any existing popup
                if (currentPopup) {
                    currentPopup.remove();
                    currentPopup = null;
                }

                // Track if spiderifier was successfully applied
                let spiderifierApplied = false;

                // Get all points in this cluster
                map.getSource('restaurants').getClusterLeaves(
                    clusterId,
                    pointCount, // limit - get all points
                    0, // offset
                    function (err, clusterFeatures) {
                        if (err) {
                            console.error('Error getting cluster leaves:', err);
                            fallbackToZoom();
                            return;
                        }

                        console.log(`Retrieved ${clusterFeatures.length} points from cluster`);

                        // Check if we have spiderifier and at least 2 features
                        if (spiderifier && clusterFeatures.length > 1) {
                            // Apply spiderifier to show all points in this cluster
                            try {
                                // Create LngLat object from cluster center coordinates
                                const centerLngLat = new mapboxgl.LngLat(clusterCenter[0], clusterCenter[1]);

                                // Unspiderify first in case there's an existing spider
                                spiderifier.unspiderfy();

                                // Spiderify the cluster features
                                spiderifier.spiderfy(centerLngLat, clusterFeatures);
                                console.log('Spiderified cluster points successfully');
                                spiderifierApplied = true;

                                // Create a function to handle unspiderifying
                                const unspiderifyWhenClickingElsewhere = function (e) {
                                    // Don't process if spiderifier is not active
                                    if (!document.querySelector('.spidered-marker-container').style.display === 'block') {
                                        map.off('click', unspiderifyWhenClickingElsewhere);
                                        return;
                                    }

                                    // Check if the click isn't on a spider leg
                                    const spiderLegs = document.querySelectorAll('.spider-leg-pin');
                                    let clickedOnSpiderLeg = false;

                                    spiderLegs.forEach(leg => {
                                        if (e.originalEvent && e.originalEvent.target === leg) {
                                            clickedOnSpiderLeg = true;
                                        }
                                    });

                                    if (!clickedOnSpiderLeg) {
                                        spiderifier.unspiderfy();
                                        map.off('click', unspiderifyWhenClickingElsewhere);
                                    }
                                };

                                // Remove any existing click handlers and add new one
                                map.off('click', unspiderifyWhenClickingElsewhere);
                                map.on('click', unspiderifyWhenClickingElsewhere);

                                // Also unspiderify on map movement events
                                const onMapMove = function () {
                                    spiderifier.unspiderfy();
                                    map.off('movestart', onMapMove);
                                    map.off('zoomstart', onMapMove);
                                    map.off('dragstart', onMapMove);
                                };

                                map.off('movestart', onMapMove);
                                map.off('zoomstart', onMapMove);
                                map.off('dragstart', onMapMove);

                                map.on('movestart', onMapMove);
                                map.on('zoomstart', onMapMove);
                                map.on('dragstart', onMapMove);

                            } catch (error) {
                                console.error('Error applying spiderifier to cluster:', error);
                                fallbackToZoom();
                            }
                        } else {
                            fallbackToZoom();
                        }
                    }
                );

                // Helper function for fallback zoom behavior
                function fallbackToZoom() {
                    if (spiderifierApplied) return; // Don't zoom if spiderifier was successfully applied

                    console.log('Falling back to cluster zoom behavior');
                    map.getSource('restaurants').getClusterExpansionZoom(
                        clusterId,
                        function (err, zoom) {
                            if (err) {
                                console.error('Error getting cluster expansion zoom:', err);
                                return;
                            }

                            // Zoom to the cluster with a slight zoom increase for better visibility
                            map.easeTo({
                                center: clusterCenter,
                                zoom: zoom + 0.5
                            });
                        }
                    );
                }
            });

            // Change cursor on cluster hover
            map.on('mouseenter', 'clusters', function () {
                map.getCanvas().style.cursor = 'pointer';
            });

            map.on('mouseleave', 'clusters', function () {
                map.getCanvas().style.cursor = '';
            });

            // Add individual points layer for non-clustered points
            map.addLayer({
                id: 'unclustered-point',
                type: 'circle',
                source: 'restaurants',
                filter: ['!', ['has', 'point_count']],
                paint: {
                    'circle-color': '#dc3545',
                    'circle-radius': 15,
                    'circle-stroke-width': 2,
                    'circle-stroke-color': '#fff'
                }
            });

            // Add number labels to individual points
            map.addLayer({
                id: 'unclustered-point-label',
                type: 'symbol',
                source: 'restaurants',
                filter: ['!', ['has', 'point_count']],
                layout: {
                    'text-field': ['get', 'number'],
                    'text-font': ['DIN Offc Pro Medium', 'Arial Unicode MS Bold'],
                    'text-size': 12
                },
                paint: {
                    'text-color': '#ffffff'
                }
            });

            // Handle clicks on individual points
            map.on('click', 'unclustered-point', function (e) {
                // Show popup for clicked point
                const features = map.queryRenderedFeatures(e.point, { layers: ['unclustered-point'] });
                if (!features.length) return;

                const feature = features[0];

                // Close any existing popup
                if (currentPopup) {
                    currentPopup.remove();
                    currentPopup = null;
                }

                // Create and display popup
                const popup = new mapboxgl.Popup({
                    closeButton: true,
                    closeOnClick: true
                }).setLngLat(feature.geometry.coordinates)
                    .setHTML(feature.properties.popupContent)
                    .addTo(map);

                currentPopup = popup;
            });

            // Change cursor on point hover
            map.on('mouseenter', 'unclustered-point', function () {
                map.getCanvas().style.cursor = 'pointer';
            });

            map.on('mouseleave', 'unclustered-point', function () {
                map.getCanvas().style.cursor = '';
            });
        }
    }
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

    // Clean up spiderifier
    cleanupSpiderifier();
}

// Function to properly clean up the spiderifier
function cleanupSpiderifier() {
    // Clear any active spiderifier markers
    if (spiderifier) {
        try {
            console.log('Cleaning up spiderifier...');
            spiderifier.unspiderfy();

            // Remove event listeners from map
            if (map) {
                map.off('click', unspiderifyWhenClickingElsewhere);
                map.off('movestart', onMapMove);
                map.off('zoomstart', onMapMove);
                map.off('dragstart', onMapMove);
            }
        } catch (error) {
            console.warn('Error cleaning up spiderifier:', error);
        }
    }
}

// These function definitions are used by the cleanupSpiderifier function
// They're empty here because they'll be set up when actually needed
function unspiderifyWhenClickingElsewhere() { }
function onMapMove() { }

// Reload map functionality
document.addEventListener('DOMContentLoaded', function () {
    const reloadButton = document.getElementById('reload-map-button');
    if (reloadButton) {
        reloadButton.addEventListener('click', function () {
            this.style.display = 'none';
            initializeMap();
        });
    }
});

// Handle window resize
window.addEventListener('resize', function () {
    if (map) {
        setTimeout(() => {
            map.resize();
        }, 100);
    }
});

// Function to focus map on a restaurant
function centerMapOnRestaurant(restaurantId) {
    const card = document.querySelector(`.restaurant-item[data-id="${restaurantId}"]`);
    if (!card || !map) {
        console.warn(`Card for restaurant ID ${restaurantId} not found or map not initialized`);
        return false;
    }

    // Unspiderify if needed
    if (spiderifier) {
        spiderifier.unspiderfy();
    }

    const lat = parseFloat(card.dataset.lat);
    const lng = parseFloat(card.dataset.lng);

    if (lat && lng && !isNaN(lat) && !isNaN(lng)) {
        console.log(`Centering map on restaurant: ${restaurantId} at (${lat}, ${lng})`);

        // Move map to restaurant location
        map.flyTo({
            center: [lng, lat],
            zoom: 16,
            duration: 1000
        });

        // Wait for map movement to complete before showing popup
        setTimeout(() => {
            // Close any existing popup
            if (currentPopup) {
                currentPopup.remove();
                currentPopup = null;
            }

            // Find feature for this restaurant
            if (map.getSource('restaurants')) {
                const features = map.querySourceFeatures('restaurants', {
                    filter: ['==', ['get', 'id'], restaurantId.toString()]
                });

                if (features.length > 0) {
                    const feature = features[0];

                    const popup = new mapboxgl.Popup({
                        closeButton: true,
                        closeOnClick: true
                    }).setLngLat([lng, lat])
                        .setHTML(createPopupContent(
                            feature.properties.name,
                            feature.properties.rating,
                            feature.properties.reviewCount,
                            feature.properties.address,
                            restaurantId
                        ))
                        .addTo(map);

                    currentPopup = popup;
                } else {
                    // Fallback if feature not found in source
                    const name = card.dataset.name;
                    const rating = card.dataset.rating;
                    const reviewCount = card.dataset.reviewCount;
                    const address = card.dataset.address;

                    const popup = new mapboxgl.Popup({
                        closeButton: true,
                        closeOnClick: true
                    }).setLngLat([lng, lat])
                        .setHTML(createPopupContent(name, rating, reviewCount, address, restaurantId))
                        .addTo(map);

                    currentPopup = popup;
                }
            }
        }, 1200); // Wait a bit after the map animation

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
        newCard.addEventListener('click', function (event) {
            // Only handle clicks on the card itself, not on links or buttons
            if (event.target.tagName.toLowerCase() === 'a' ||
                event.target.tagName.toLowerCase() === 'button') {
                return; // Let the link or button handle the click
            }

            const restaurantId = this.dataset.id;
            console.log(`Card clicked for restaurant ID: ${restaurantId}`);

            // Focus map on restaurant and show popup
            centerMapOnRestaurant(restaurantId);
        });
    });
    console.log('Restaurant card handlers initialized with', restaurantCards.length, 'cards');
}

// Initialize card handlers when page loads
document.addEventListener('DOMContentLoaded', function () {
    // Wait a bit for the map and features to be ready
    setTimeout(initializeRestaurantCardHandlers, 500);
});

// Global function to be called after AJAX updates
window.loadRestaurantsFromList = loadRestaurantsFromList;
window.centerMapOnRestaurant = centerMapOnRestaurant;
window.initializeRestaurantCardHandlers = initializeRestaurantCardHandlers;
