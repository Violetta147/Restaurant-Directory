/* Mapbox GL Spiderifier
 * Library to create beautiful "spider" legs for multiple markers at the same location
 * https://github.com/bewithjonam/mapboxgl-spiderifier
 * Author: Vincent Delecroix
 * License: MIT
 */

function MapboxglSpiderifier(map, options) {
    this._map = map;
    this._options = options || {};
    this._options.animate = this._options.animate || true;
    this._options.animationSpeed = this._options.animationSpeed || 200;
    this._options.customPin = this._options.customPin || false;
    this._options.initializeLeg = this._options.initializeLeg || null;
    this._options.onClick = this._options.onClick || null;
    this._options.radius = this._options.radius || 100;
    this._options.circleFootSeparation = this._options.circleFootSeparation || 40;
    this._options.spiralFootSeparation = this._options.spiralFootSeparation || 30;
    this._options.spiralLengthStart = this._options.spiralLengthStart || 10;
    this._options.spiralLengthFactor = this._options.spiralLengthFactor || 4;

    this._initSpiderDom();
    this._events = {};
    this._features = [];
    this._featureById = {};
    this._selectedfeature = null;

    this._bind();
}

MapboxglSpiderifier.prototype = {
    destroy: function() {
        if (this._domContainer.parentNode) {
            this._domContainer.parentNode.removeChild(this._domContainer);
        }
        this._options = null;
        this._map = null;
        this._domContainer = null;
        this._domList = null;
        this._features = null;
        this._featureById = null;
        this._selectedfeature = null;
    },

    _initSpiderDom: function() {
        this._domContainer = document.createElement('DIV');
        this._domContainer.setAttribute('class', 'spidered-marker-container');

        var handleContainer = document.createElement('DIV');
        handleContainer.setAttribute('class', 'spider-leg-container');

        this._domList = document.createElement('UL');
        this._domList.setAttribute('class', 'spider-leg-list');

        handleContainer.appendChild(this._domList);
        this._domContainer.appendChild(handleContainer);
        this._map._container.appendChild(this._domContainer);
    },    _removeAllLegs: function() {
        // Remove all child elements from domList to clean up completely
        while (this._domList.firstChild) {
            this._domList.removeChild(this._domList.firstChild);
        }
    },_processLegPosition: function(positions) {
        // Process and return leg positions (currently just returns the original positions)
        // This function could be used for additional transformations if needed in the future
        return positions;
    },_convertPosition: function(latlng) {
        // Project the coordinates to pixel space
        var point = this._map.project(latlng);
        // Get map container's position for offset calculation
        var container = this._map.getContainer();
        var rect = container.getBoundingClientRect();
        
        // Return the point without additional offset - Mapbox GL already handles this
        return point;
    },

    _calulcatePositions: function(count) {
        var res = [];
        
        if (count === 0) {
            return res;
        }
        
        // If only one marker, position it above the original point
        if (count === 1) {
            res.push({
                angle: 0,
                leg: this._options.circleFootSeparation,
                x: 0,
                y: -this._options.circleFootSeparation, // position above
            });
            return res;
        }
        
        // Use a spiral for many points
        if (count > 8) {
            var legLength = this._options.spiralLengthStart;
            var angleStep = Math.PI * 2 / this._options.spiralFootSeparation;
            
            // Start angle slightly to the right to avoid exact vertical positioning for first element
            var angle = Math.PI / 6;
            
            for (var i = 0; i < count; i++) {
                res.push({
                    angle: angle,
                    leg: legLength,
                    x: legLength * Math.cos(angle),
                    y: legLength * Math.sin(angle),
                });
                angle += angleStep;
                legLength += this._options.spiralFootSeparation / angleStep;
            }
            
            return res;
        }
        
        // For 2-8 markers, use a circle arrangement
        var circumference = this._options.radius * 2 * Math.PI;
        var legLength = this._options.circleFootSeparation;
        var angleStep = (Math.PI * 2) / count;
        
        // Start angle slightly to the right to avoid exact vertical positioning for first element
        var angle = Math.PI / 6;
        
        for (var i = 0; i < count; i++) {
            res.push({
                angle: angle,
                leg: legLength,
                x: legLength * Math.cos(angle),
                y: legLength * Math.sin(angle),
            });
            angle += angleStep;
        }
        
        return res;
    },
    
    _bind: function() {
        // No-op if onClick not provided
        if (!this._options.onClick) return;
        
        // Otherwise, find elements and bind them
        var legs = this._domList.querySelectorAll('.spider-leg-pin');
        for(var i=0; i<legs.length; i++) {
            var leg = legs[i];
            leg.addEventListener('click', this._handleClick.bind(this));
        }
    },
      _handleClick: function(e) {
        var target = e.target;
        var featureId = target.getAttribute('data-feature-id');
        if (featureId && this._featureById[featureId]) {
            var feature = this._featureById[featureId];
            
            // Add the pin position to the event so the click handler can use it
            e.pinPosition = {
                left: parseFloat(target.style.left),
                top: parseFloat(target.style.top),
                clientX: e.clientX,
                clientY: e.clientY
            };
            
            this._options.onClick(e, feature);
        }
    },
      spiderfy: function(lngLat, features) {
        if (!lngLat || !features || features.length === 0) {
            console.log('No features to spiderify');
            return;
        }
        
        console.log(`Spiderify called with ${features.length} features at ${lngLat.lng}, ${lngLat.lat}`);
        
        this.unspiderfy();
        
        this._features = features;
        this._featureById = {}; // Clear existing features
        
        // Store features by ID for quick lookup
        for (var i = 0; i < features.length; i++) {
            var feature = features[i];
            var id = feature.properties && feature.properties.id ? feature.properties.id : i;
            this._featureById[id] = feature;
        }
        
        // Calculate positions for each leg
        var positions = this._calulcatePositions(features.length);
        
        // Create legs
        var point = this._convertPosition(lngLat);
        console.log('Central point for spiderifier:', point);
        
        for (var i = 0; i < features.length; i++) {
            var pos = positions[i];
            var feature = features[i];
            var id = feature.properties && feature.properties.id ? feature.properties.id : i;
              // Create the connector line element
            var listElement = document.createElement('LI');
            listElement.setAttribute('class', 'spider-leg-container');
            
            // Calculate the line length and rotation angle
            var lineLength = Math.sqrt(pos.x * pos.x + pos.y * pos.y);
            var angle = Math.atan2(pos.y, pos.x) * 180 / Math.PI;
            
            // Position and style the line element
            listElement.style.width = lineLength + 'px';
            listElement.style.transform = `rotate(${angle}deg)`;
            listElement.style.left = point.x + 'px';
            listElement.style.top = point.y + 'px';
            
            // Create the marker pin element
            var pinElement = document.createElement('DIV');
            pinElement.setAttribute('class', 'spider-leg-pin');
            pinElement.setAttribute('data-feature-id', id);
            
            // Apply custom pin styling if available
            if (this._options.initializeLeg) {
                this._options.initializeLeg(pinElement, feature);
            } else {
                // Default styling if no custom styling provided
                pinElement.style.backgroundColor = '#dc3545';
                pinElement.style.border = '2px solid white';
                pinElement.style.width = '30px';
                pinElement.style.height = '30px';
                pinElement.style.borderRadius = '50%';
                pinElement.style.textAlign = 'center';
                pinElement.style.lineHeight = '26px';
                pinElement.style.color = 'white';
                pinElement.style.fontWeight = 'bold';
                pinElement.innerText = (i + 1).toString();
            }            // Position the pin at the end of the line with improved centering
            const pinSize = 36; // Match the width/height in the styling (36px)
            const halfPinSize = pinSize / 2;
            pinElement.style.left = `${point.x + pos.x - halfPinSize}px`; // Center horizontally
            pinElement.style.top = `${point.y + pos.y - halfPinSize}px`; // Center vertically
            
            // Store the original coordinates as data attributes for popup positioning
            pinElement.setAttribute('data-lng', feature.geometry.coordinates[0]);
            pinElement.setAttribute('data-lat', feature.geometry.coordinates[1]);
            
            // IMPORTANT: Ensure the original coordinates are accessible in the popup
            // Clone them to ensure they're not lost or altered during event handling
            if (!feature.originalLngLat) {
                feature.originalLngLat = {
                    lng: feature.geometry.coordinates[0],
                    lat: feature.geometry.coordinates[1]
                };
            }
            
            // Add elements to DOM
            this._domList.appendChild(listElement);
            this._domList.appendChild(pinElement);
        }
        
        // Show the container
        this._domContainer.style.display = 'block';
        
        // Bind events to new legs
        this._bind();
        
        console.log('Spiderify completed with', features.length, 'legs');
    },
    
    unspiderfy: function() {
        // Hide container
        this._domContainer.style.display = 'none';
        
        // Remove all legs
        this._removeAllLegs();
        
        // Clear data
        this._features = [];
        this._featureById = {};
        this._selectedfeature = null;
    }
};
