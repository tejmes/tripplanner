// trips-overview.js
window.initTripAutocomplete = function () {
    const input = document.getElementById('destination-autocomplete');
    if (!input) return console.error('Input neexistuje');

    const ac = new google.maps.places.Autocomplete(input, {
        fields: ['place_id', 'name', 'address_components', 'geometry'],
        types: ['(cities)']
    });

    ac.addListener('place_changed', () => {
        const p = ac.getPlace();
        if (!p.place_id || !p.geometry) {
            return console.error('Chybí place_id nebo geometry');
        }

        const lat = p.geometry.location.lat();
        const lng = p.geometry.location.lng();

        document.querySelector('[name="DestinationLocation.GooglePlaceId"]').value = p.place_id;
        document.querySelector('[name="DestinationName"]').value = p.name;
        const country = p.address_components.find(c => c.types.includes('country'));
        document.querySelector('[name="Country"]').value = country?.long_name || '';

        document.querySelector('[name="DestinationLocation.Latitude"]').value = lat.toLocaleString('cs-CZ', { useGrouping: false });
        document.querySelector('[name="DestinationLocation.Longitude"]').value = lng.toLocaleString('cs-CZ', { useGrouping: false });
    });
};
