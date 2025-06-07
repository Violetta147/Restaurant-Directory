/**
 * MarkerSpider for Mapbox GL JS
 * 
 * A simple "spiderfy" implementation for Mapbox GL that spreads out markers that are too close together.
 * Inspired by Leaflet.MarkerCluster
 */
class MarkerSpider {
    constructor(map, options = {}) {
        this.map = map;
        this.markers = [];
        this.spiderfied = false;
        this.spiderLegs = [];
        this.spiderParams = {
            circleSpiralSwitchover: 9,  // After this number of markers, switch from circle to spiral pattern
            circleFootSeparation: 40,    // Separation between markers in circle pattern
            spiralFootSeparation: 30,    // Separation between markers in spiral pattern
            spiralLengthStart: 20,       // Start length of spiral
            spiralLengthFactor: 4,       // Factor by which spiral grows with each additional element
            animate: true,              // Whether to animate the spiderfy
            animationDuration: 200,      // Duration of animation in ms
            ...options
        };

        // Close spiderfied markers when map is clicked or moved
        this.map.on('click', this.unspiderfy.bind(this));
        this.map.on('movestart', this.unspiderfy.bind(this));
        this.map.on('zoomstart', this.unspiderfy.bind(this));
    }

    // Add a marker to be tracked
    addMarker(marker, id) {
        this.markers.push({
            marker: marker,
            id: id,
            originalLngLat: marker.getLngLat()
        });
    }

    // Clear all tracked markers
    clearMarkers() {
        // Make sure we unspiderfy first
        this.unspiderfy();
        this.markers = [];
    }

    // Find markers that are too close to each other at the current zoom level
    findOverlappingMarkers() {
        const overlappingGroups = [];
        const processedIds = new Set();

        // Calculate pixel position for each marker
        const markerPixelPositions = this.markers.map(m => {
            const lngLat = m.originalLngLat || m.marker.getLngLat();
            const point = this.map.project(lngLat);
            return {
                id: m.id,
                marker: m.marker,
                pixelPos: point
            };
        });
        
        // Find groups of overlapping markers
        for (let i = 0; i < markerPixelPositions.length; i++) {
            // Skip if this marker has been processed already
            if (processedIds.has(markerPixelPositions[i].id)) continue;

            const currentGroup = [markerPixelPositions[i]];
            processedIds.add(markerPixelPositions[i].id);
            
            for (let j = i + 1; j < markerPixelPositions.length; j++) {
                // Skip if this marker has been processed already
                if (processedIds.has(markerPixelPositions[j].id)) continue;
                
                // Check if markers are too close (less than 30 pixels apart)
                const distance = Math.sqrt(
                    Math.pow(markerPixelPositions[i].pixelPos.x - markerPixelPositions[j].pixelPos.x, 2) +
                    Math.pow(markerPixelPositions[i].pixelPos.y - markerPixelPositions[j].pixelPos.y, 2)
                );
                
                if (distance < 30) {
                    currentGroup.push(markerPixelPositions[j]);
                    processedIds.add(markerPixelPositions[j].id);
                }
            }
            
            // If we found more than one marker in this group
            if (currentGroup.length > 1) {
                overlappingGroups.push(currentGroup);
            }
        }
        
        return overlappingGroups;
    }

    // Generate positions in a spiral pattern
    generateSpiralPositions(center, count) {
        const { spiralFootSeparation, spiralLengthStart, spiralLengthFactor } = this.spiderParams;
        const positions = [];
        
        // Start angle (in radians)
        let legLength = spiralLengthStart;
        let angle = 0;
        
        for (let i = 0; i < count; i++) {
            // Angle is continually increased to create the spiral
            angle = angle + (2 * Math.PI / count);
            
            // Calculate the new position
            const x = center.x + Math.cos(angle) * legLength;
            const y = center.y + Math.sin(angle) * legLength;
            
            positions.push({ x, y });
            
            // Increase leg length for next position
            legLength = legLength + (spiralLengthFactor * spiralFootSeparation / Math.PI);
        }
        
        return positions;
    }

    // Generate positions in a circle pattern
    generateCirclePositions(center, count) {
        const { circleFootSeparation } = this.spiderParams;
        const positions = [];
        
        const circumference = circleFootSeparation * count;
        const radius = circumference / (2 * Math.PI);
        
        for (let i = 0; i < count; i++) {
            const angle = (i / count) * (2 * Math.PI);
            const x = center.x + Math.cos(angle) * radius;
            const y = center.y + Math.sin(angle) * radius;
            
            positions.push({ x, y });
        }
        
        return positions;
    }
    
    // Spiderfy a group of markers
    spiderfy(markerGroup) {
        // Unspiderfy any currently spiderfied markers first
        if (this.spiderfied) {
            this.unspiderfy();
        }
        
        if (!markerGroup || markerGroup.length <= 1) {
            return;
        }
        
        // Use the first marker's position as the center
        const centerPoint = markerGroup[0].pixelPos;
        const count = markerGroup.length;
        const { circleSpiralSwitchover, animate, animationDuration } = this.spiderParams;
        
        // Generate positions for the markers
        let positions;
        if (count < circleSpiralSwitchover) {
            positions = this.generateCirclePositions(centerPoint, count);
        } else {
            positions = this.generateSpiralPositions(centerPoint, count);
        }
        
        // Move the markers to their new positions
        for (let i = 0; i < count; i++) {
            const marker = markerGroup[i].marker;
            const pixelPosition = positions[i];
            const newLngLat = this.map.unproject(pixelPosition);
            
            // Create a line from the center to the new position
            if (i > 0) { // Skip the center marker
                const leg = this.createSpiderLeg(
                    this.map.unproject(centerPoint),
                    newLngLat,
                    `spider-leg-${markerGroup[i].id}`
                );
                this.spiderLegs.push(leg);
            }
            
            // Move the marker
            if (animate) {
                this.animateMarkerMove(marker, newLngLat, animationDuration);
            } else {
                marker.setLngLat(newLngLat);
            }
        }
        
        this.spiderfied = markerGroup[0].id;
    }

    // Create a line between two points (spider leg)
    createSpiderLeg(startLngLat, endLngLat, id) {
        // Remove any existing spider leg with this ID
        if (this.map.getLayer(id)) {
            this.map.removeLayer(id);
        }
        if (this.map.getSource(id)) {
            this.map.removeSource(id);
        }
        
        // Create a new source and layer for this leg
        this.map.addSource(id, {
            'type': 'geojson',
            'data': {
                'type': 'Feature',
                'properties': {},
                'geometry': {
                    'type': 'LineString',
                    'coordinates': [
                        [startLngLat.lng, startLngLat.lat],
                        [endLngLat.lng, endLngLat.lat]
                    ]
                }
            }
        });
        
        this.map.addLayer({
            'id': id,
            'type': 'line',
            'source': id,
            'layout': {
                'line-join': 'round',
                'line-cap': 'round'
            },
            'paint': {
                'line-color': '#666',
                'line-opacity': 0.6,
                'line-width': 1.5
            }
        });
        
        return id;
    }

    // Animate marker movement
    animateMarkerMove(marker, newLngLat, duration) {
        const startLngLat = marker.getLngLat();
        const startTime = Date.now();
        const endTime = startTime + duration;
        
        const animate = () => {
            const now = Date.now();
            const progress = Math.min((now - startTime) / duration, 1);
            
            // Ease function (ease out quad)
            const easeOut = progress * (2 - progress);
            
            // Calculate the current position
            const currentLng = startLngLat.lng + (newLngLat.lng - startLngLat.lng) * easeOut;
            const currentLat = startLngLat.lat + (newLngLat.lat - startLngLat.lat) * easeOut;
            
            // Update the marker position
            marker.setLngLat([currentLng, currentLat]);
            
            // Continue animation if not done
            if (now < endTime) {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    }

    // Unspiderfy currently spiderfied markers
    unspiderfy() {
        if (!this.spiderfied) {
            return;
        }
        
        // Remove all spider legs
        this.spiderLegs.forEach(legId => {
            if (this.map.getLayer(legId)) {
                this.map.removeLayer(legId);
            }
            if (this.map.getSource(legId)) {
                this.map.removeSource(legId);
            }
        });
        this.spiderLegs = [];
        
        // Reset all marker positions
        this.markers.forEach(marker => {
            if (marker.originalLngLat) {
                if (this.spiderParams.animate) {
                    this.animateMarkerMove(marker.marker, marker.originalLngLat, this.spiderParams.animationDuration);
                } else {
                    marker.marker.setLngLat(marker.originalLngLat);
                }
            }
        });
        
        this.spiderfied = false;
    }
}
