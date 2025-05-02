let map;
let marker;
let debounceTimer;
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
function initializeMap(lat = 10.762622, lon = 106.660172, name) { // Default: Ho Chi Minh City center
    if (map) {
        map.remove();
        map = null;
        marker = null;
    }
    
    map = L.map('map').setView([lat, lon], 13);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
    
    marker = L.marker([lat, lon], { draggable: true }).addTo(map);
    
    // console.log(lat);
    // console.log(lon);
    // console.log(name);

    if (lat === 0 && lon === 0) {
        searchLocation(name)
    } else {
        // Set initial Lat/Lon values
        const position = marker.getLatLng();

        let input_Lat = document.getElementById('Latitude');
        let input_Lon = document.getElementById('Longitude');
        if (input_Lat) input_Lat.value = position.lat;
        if (input_Lon) input_Lon.value = position.lng;
    }

    // Update hidden fields when marker is dragged
    marker.on('dragend', function () {
        const position = marker.getLatLng();
        handleDebounce(this, async () => reverseGeoCode(position.lat, position.lng), 500);
    });
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
    if (!lat || !lon) {
        console.error(lat);
        console.error(lon);
        console.error('Latitude or longitude is null.');
        return null;
    }
    if (isNaN(lat) || isNaN(lon)) {
        console.error('Invalid latitude or longitude.');
        return null;
    }

    try {
        const vnUrl = `${baseUrl}${urlReverse}lat=${lat}&lon=${lon}${dataJsonFormat}${dataLanguage}`;
        const enUrl = `${baseUrl}${urlReverse}lat=${lat}&lon=${lon}${dataJsonFormat}&accept-language=en`;
        console.log(`Requesting reverse geocode for lat: ${lat}, lon: ${lon}`);
        
        const vnResponse = await fetchFromAPI(vnUrl);

        let result = vnResponse;

        if (!vnResponse || !vnResponse.display_name) {
            console.warn("Vietnamese not available, falling back to English");
            result = await fetchFromAPI(enUrl);
        } 

        if (result) {
            updateFormAndMarkerFromSearch(vnResponse);
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

        let result = vnResponse;

        if (!vnResponse || !vnResponse.display_name) {
            console.warn("Vietnamese not available, falling back to English");
            result = await fetchFromAPI(enUrl);
        } 

        if (result) {
            updateFormAndMarkerFromSearch(vnResponse);
        } else {
            console.error("No data returned from reverese geocode.")
        }
    } catch (error) {
        console.error('Error searching location:', error);
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

async function addRouteAndTime(startLat, startLon, endLat, endLon) {
    const routeUrl = `${osrmUrl}${startLon},${startLat};${endLon},${endLat}?overview=full&geometries=geojson&steps=true`;

    try {
        const response = await fetch(routeUrl);
        const data = await response.json();

        if (data.routes && data.routes[0]) {
            const route = data.routes[0];
            const routeCoords = route.geometry.coordinates.map(coord => [coord[1], coord[0]]); // Convert [lon, lat] to [lat, lon]
            console.log('Number of route coordinates:', route.geometry.coordinates.length);

            L.polyline(routeCoords, { color: 'blue', weight: 4 }).addTo(map);
            
            const startLocation = await reverseGeoCode(startLat, startLon);
            const endLocation = await reverseGeoCode(endLat, endLon);

            
            L.marker([startLat, startLon]).addTo(map).bindPopup(`Start: ${startLocation.display_name}`);
            L.marker([endLat, endLon]).addTo(map).bindPopup(`End: ${endLocation.display_name}`);
            
            const duration = route.duration;
            const hours = Math.floor(duration / 3600);
            const minutes = Math.floor((duration % 3600) / 60);
            const travelTime = `${hours} hours ${minutes} minutes`;
            
            const popupContent = `<b>Estimated travel time:</b><br>${travelTime}`;
            L.popup()
            .setLatLng([(startLat + endLat) / 2, (startLon + endLon) / 2])
            .setContent(popupContent)
            .openOn(map);
            
            const routeBounds = L.latLngBounds(routeCoords);
            map.fitBounds(routeBounds, { padding: [10, 10] }); 
            console.log(startLocation);
            console.log(endLocation);
            return { startLocation, endLocation };
        }
    } catch (error) {
        console.error('Error fetching route:', error);
        return { startLocation: null, endLocation: null };
    }
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