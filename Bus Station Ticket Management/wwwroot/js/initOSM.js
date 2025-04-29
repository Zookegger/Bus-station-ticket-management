let map;
let marker;
let debounceTimer;
let amount = 1;
const baseUrl = `https://nominatim.openstreetmap.org/`;
const urlSearchQuery = `search?q=`;
const urlReverse = `reverse?`;
const dataJsonFormat = `&format=json`;
const dataLimit = `&limit=${amount}`;
const fetchHeaders = {
    'User-Agent': 'Mozilla/5.0 (Compatible; AcmeInc/1.0)' // Required by Nominatim
}

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
        document.getElementById('Latitude').value = position.lat;
        document.getElementById('Longitude').value = position.lng;
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
        return;
    }
    if (isNaN(lat) || isNaN(lon)) {
        console.error('Invalid latitude or longitude.');
        return;
    }

    try {
        const url = `${baseUrl}${urlReverse}lat=${lat}&lon=${lon}${dataJsonFormat}`;
        console.log(`Requesting reverse geocode for lat: ${lat}, lon: ${lon}`);
        
        const response = await fetchFromAPI(url);

        if (response) {
            updateFormAndMarkerFromSearch(response);
        } else {
            console.error('No data returned from reverse geocode.');
        }
    } catch (error) {
        console.error(error);
        return null;
    }
}

async function searchLocation(location) {
    if (!location) return;

    try {
        const url = `${baseUrl}${urlSearchQuery}${encodeURIComponent(location)}${dataJsonFormat}${dataLimit}`;
        console.log(`Searching for location: ${location}`);

        const response = await fetchFromAPI(url);

        if (response) {
            updateFormAndMarkerFromSearch(response);
        } else {
            console.error('No data returned from reverse geocode.');
        }
    } catch (error) {
        console.error('Error searching location:', error);
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