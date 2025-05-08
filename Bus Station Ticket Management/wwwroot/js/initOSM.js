let map;
let marker;
let debounceTimer;
let currentRoute = null;
let startMarker = null;
let endMarker = null;
let amount = 1;
let language = 'vi';
const baseUrl = `https://nominatim.openstreetmap.org/`;
const urlSearchQuery = `search?q=`;
const urlReverse = `reverse?`;
const dataLanguage = `&accept-language=${language}`;
const dataJsonFormat = `&format=json`;
const dataLimit = `&limit=${amount}`;
const fetchHeaders = {
    'User-Agent': 'Mozilla/5.0 (Compatible; AcmeInc/1.0)' // Required by Nominatim
}

const osrmUrl = `https://router.project-osrm.org/route/v1/driving/`;

// Initialize the map
function initializeMap(lat = 10.762622, lon = 106.660172, name, draggableMarker = true, zoom = 13) { // Default: Ho Chi Minh City center
    // if (map) {
    //     map.remove();
    //     map = null;
    //     marker = null;
    // }
    try {
        map = L.map('map').setView([lat, lon], zoom);
        
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);
        
        const key = `reverse:${lat},${lon}`;
        const cached = loadWithExpiry(key);
        if (cached) {
            marker = L.marker([cached.lat, cached.lon], { draggable: draggableMarker }).addTo(map);
            return cached;
        }
        
        marker = L.marker([lat, lon], { draggable: draggableMarker }).addTo(map);
        
        // console.log(lat);
        // console.log(lon);
        // console.log(name);

        if (lat === 0 && lon === 0) {
            searchLocation(name)
        } else {
            // Set initial Lat/Lon values
            if (marker) {
                const position = marker.getLatLng();
                
                let input_Lat = document.getElementById('Latitude');
                let input_Lon = document.getElementById('Longitude');
                if (input_Lat) input_Lat.value = position.lat;
                if (input_Lon) input_Lon.value = position.lng;
            }
        }
        if (marker) {
            // Update hidden fields when marker is dragged
            marker.on('dragend', function () {
                const position = marker.getLatLng();
                console.log(position);
                handleDebounce(this, async () => reverseGeoCode(position.lat, position.lng), 500);
            });
        }
    } catch (error) {
        console.error(error);
    }
}

function cleanExpiredCache() {
    const now = new Date().getTime();

    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        const item = localStorage.getItem(key);

        try {
            const data = JSON.parse(item);
            if (data && data.expiry && now > data.expiry) {
                console.log(`Data clear ${key}`);
                localStorage.removeItem(key);
            }
        } catch (error) {
            console.error(error);
        }
    }
}

function saveWithExpiry(key, data, ttlMinutes = 2) {
    const now = new Date();
    const item = {
        value: data,
        expiry: now.getTime() + ttlMinutes * 60 * 1000
    }
    try {
        localStorage.setItem(key, JSON.stringify(item));
    } catch (e) {
        if (e.name === 'QuotaExceededError') {
            // Optionally: localStorage.clear(); // or remove oldest items
            console.warn('LocalStorage quota exceeded, cannot cache:', key);
        } else {
            throw e;
        }
    }
}

function loadWithExpiry(key) {
    const itemStr = localStorage.getItem(key);
    if (!itemStr) return null;

    try {
        const item = JSON.parse(itemStr);
        const now = new Date();

        if (now.getTime() > item.expiry) {
            localStorage.removeItem(key);
            return null;
        }

        return item.value;
    } catch (e) {
        console.error('Invalid cached data format:', e);
        localStorage.removeItem(key);
        return null;
    }
}

function cleanDisplayName(name, displayName) {
    if (displayName.startsWith(name)) {
        return displayName.substring(name.length).replace(/^,\s*/, ''); // remove name and following comma+space
    }
    return displayName;
}

function handleDebounce(event, callback, delay) {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(callback, delay);
}

function updateFormAndMarkerFromSearch(data) {
    // console.log(data)
    let item;

    if (data.length > 0) {
        item = data[0];
    } else {
        item = data;
    }
    
    const name = item.name;
    const displayName = item.display_name;
    // console.log(name);
    // console.log(displayName);

    const searchName = document.getElementById("searchName");
    const searchAddress = document.getElementById("searchAddress");
    const latitudeInput = document.getElementById('Latitude');
    const longitudeInput = document.getElementById('Longitude');
    
    if (searchName) searchName.value = name;
    if (searchAddress) searchAddress.value = cleanDisplayName(name, displayName);    

    const lat = parseFloat(item.lat);
    const lon = parseFloat(item.lon);
    
    if (latitudeInput) latitudeInput.value = lat;
    if (longitudeInput) longitudeInput.value = lon;

    if (typeof marker !== "undefined" && marker) marker.setLatLng([lat, lon]);
    if (typeof map !== "undefined" && map) map.setView([lat, lon], 17);

    console.log(3);

    return { lat, lon };
}

async function fetchFromAPI(url) {
    try {
        const response = await fetch(url, { headers: fetchHeaders });
        if (!response.ok) {
            throw new Error('Failed to fetch data from API');
        }
        return await response.json();
    } catch (error) {
        console.error(error);
    }
}

async function reverseGeoCode(lat, lon) {
    const key = `reverse:${lat},${lon}`;
    const cached = loadWithExpiry(key);
    if (cached) {
        console.log(cached);
        updateFormAndMarkerFromSearch(cached);
        return cached;
    }

    if (!lat || !lon || isNaN(lat) || isNaN(lon)) {
        console.error('Invalid latitude or longitude:', lat, lon);
        return null;
    }

    try {
        const vnUrl = `${baseUrl}${urlReverse}lat=${lat}&lon=${lon}${dataJsonFormat}${dataLanguage}`;
        const enUrl = `${baseUrl}${urlReverse}lat=${lat}&lon=${lon}${dataJsonFormat}&accept-language=en`;
        
        const vnResponse = await fetchFromAPI(vnUrl);
        let result = vnResponse;

        if (!vnResponse || !vnResponse.display_name) {
            console.warn("Vietnamese not available, falling back to English");
            result = await fetchFromAPI(enUrl);
        } 

        console.log(1);

        if (result) {
            updateFormAndMarkerFromSearch(vnResponse);
            saveWithExpiry(key, result); // Cache data

            return result;
        } else {
            console.error("No data returned from reverese geocode.")
        }

    } catch (error) {
        console.error(error);
        return null;
    }
}


async function searchLocation(location) {
    if (!location) return;

    try {
        const vnUrl = `${baseUrl}${urlSearchQuery}${encodeURIComponent(location)}${dataJsonFormat}${dataLimit}&accept-language=vi}`;
        const enUrl = `${baseUrl}${urlSearchQuery}${encodeURIComponent(location)}${dataJsonFormat}${dataLimit}&accept-language=en`;

        const vnResponse = await fetchFromAPI(vnUrl);

        let result = vnResponse[0];

        if (!result || !result.display_name) {
            console.warn("Vietnamese not available, falling back to English");
            result = await fetchFromAPI(enUrl);
        } 

        if (result) {
            updateFormAndMarkerFromSearch(result);
        } else {
            console.error("No data returned from reverese geocode.")
        }
    } catch (error) {
        console.error('Error searching location:', error);
    }
}

async function searchLocationWithResult(location) {
    if (!location) {console.error('No input data'); return;}

    try {
        const vnUrl = `${baseUrl}${urlSearchQuery}${encodeURIComponent(location)}${dataJsonFormat}${dataLimit}&accept-language=vi}`;
        const enUrl = `${baseUrl}${urlSearchQuery}${encodeURIComponent(location)}${dataJsonFormat}${dataLimit}&accept-language=en`;

        const vnResponse = await fetchFromAPI(vnUrl);

        let result = vnResponse;

        console.log(result);
        console.log(result.display_name);
        
        if (result == null || result[0].display_name == null) {
            console.warn("Vietnamese not available, falling back to English");
            result = await fetchFromAPI(enUrl);
        } 
        console.log(result);

        if (result) {
            return {
                name: result[0].display_name,
                lat: parseFloat(result[0].lat),
                lng: parseFloat(result[0].lon),
            };
        } else {
            console.error("No data returned from reverese geocode.")
        }
    } catch (error) {
        console.error('Error searching location:', error);
        return null;
    }
}

async function getTravelTime(startLat, startLon, endLat, endLon) {
    const routeUrl = `${osrmUrl}${startLon},${startLat};${endLon},${endLat}?overview=full&geometries=geojson?overview=false&alternatives=false&steps=false&annotations=duration`;
    
    try {
        const response = await fetch(routeUrl);
        const data = await response.json();

        if (data.routes && data.routes[0]) {
            const duration = data.routes[0].duration; // Duration in seconds
            const hours = Math.floor(duration / 3600);
            const minutes = Math.floor((duration % 3600) / 60);
            return `${hours} hours ${minutes} minutes`;
        }
    } catch (error) {
        console.error('Error calculating travel time:', error);
        return 'Unavailable';
    }    
}

async function addRouteAndTime(startLat, startLon, endLat, endLon, moveMarker = false) {
    try {
        const key = `route:${startLat},${startLon}->${endLat},${endLon},`;
        const cached = loadWithExpiry(key);
        if (cached) {
            console.log(cached);
            drawRoute(cached, moveMarker);
            return cached;
        }
        
        const routeUrl = `${osrmUrl}${startLon},${startLat};${endLon},${endLat}?overview=full&geometries=geojson&steps=true`;

        const response = await fetch(routeUrl);
        const data = await response.json();

        if (data.routes && data.routes[0]) {
            const route = data.routes[0];
            const startLocation = await reverseGeoCode(startLat, startLon);
            const endLocation = await reverseGeoCode(endLat, endLon);

            const result = { route, startLocation, endLocation };
            saveWithExpiry(key, result); // cache it
            drawRoute(result, moveMarker);
            return result;
        }
    } catch (error) {
        console.error('Error fetching route:', error);
        return { startLocation: null, endLocation: null };
    }
}

function drawRoute(data, moveMarker = false) {
    if (currentRoute) {
        map.removeLayer(currentRoute);
        currentRoute = null;
    }
    if (startMarker) {
        map.removeLayer(startMarker);
        startMarker = null;
    }
    if (endMarker) {
        map.removeLayer(endMarker);
        endMarker = null;
    }
    
    const routeCoords = data.route.geometry.coordinates.map(coord => [coord[1], coord[0]]);
    currentRoute = L.polyline(routeCoords, { color: 'blue', weight: 4 }).addTo(map);

    const { startLocation, endLocation } = data;
    
    if (!startLocation || isNaN(startLocation.lat) || isNaN(startLocation.lon) ||
        !endLocation || isNaN(endLocation.lat) || isNaN(endLocation.lon)) {
        throw new Error("Invalid start or end location", startLocation, endLocation);
    }

    startMarker = L.marker([startLocation.lat, startLocation.lon], { draggable: moveMarker })
        .addTo(map).bindPopup(`Start: ${startLocation.display_name}`);
    endMarker = L.marker([endLocation.lat, endLocation.lon], { draggable: moveMarker })
        .addTo(map).bindPopup(`End: ${endLocation.display_name}`);

    const duration = data.route.duration;
    const hours = Math.floor(duration / 3600);
    const minutes = Math.floor((duration % 3600) / 60);
    const travelTime = `${hours} hours ${minutes} minutes`;

    let distance = data.route.distance;
    let popupContent = `<b>Estimated travel time:</b><br>${travelTime}<br><b>Total Distance</b><br>`;
    popupContent += (distance > 1000) 
        ? `${(distance / 1000).toFixed(1)} km` 
        : `${Math.round(distance)} m`;

    L.popup()
        .setLatLng([
            (parseFloat(startLocation.lat) + parseFloat(endLocation.lat)) / 2, 
            (parseFloat(startLocation.lon) + parseFloat(endLocation.lon)) / 2
        ])
        .setContent(popupContent)
        .openOn(map);

    const routeBounds = L.latLngBounds(routeCoords);
    map.fitBounds(routeBounds, { padding: [10, 10] });
}

document.addEventListener('DOMContentLoaded', async function () {    
    try {

        initializeMap(); // Just call your own function instead of creating map again
        
        const searchName = document.getElementById("searchName");
        const searchAddress = document.getElementById("searchAddress");
        
        // For Edit View
        if (searchName.value || (searchAddress && searchAddress.value.trim() != "")) {
            await reverseGeoCode(searchAddress.value.trim());
        }
        
        searchAddress.addEventListener('input', function () {
            handleDebounce(this, async () => searchLocation(this.value), 1500)
        });
        
        searchName.addEventListener('input', function () {
            handleDebounce(this, async () => searchLocation(this.value), 1500)
        });
    } catch (error) {
        console.log();
    }
});