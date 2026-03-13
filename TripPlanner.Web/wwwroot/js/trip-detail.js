function initSortable() {

    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    document.querySelectorAll('ul[id^="places-day-"]').forEach(ul => {
        const dayId = +ul.id.split('-').pop();

        Sortable.create(ul, {
            animation: 150,
            group: { name: 'places', pull: true, put: true },
            draggable: '.place-item',
            ghostClass: 'sortable-ghost',

            onAdd: async evt => {
                const pid = +evt.item.dataset.id;
                const nd = +evt.to.id.split('-').pop();
                if (!pid || !nd) return;

                await fetch('/User/Place/Move', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': csrfToken
                    },
                    body: JSON.stringify({ placeId: pid, newTripDayId: nd })
                });

                const newO = Array.from(evt.to.querySelectorAll('.place-item')).map(li => +li.dataset.id);
                await fetch('/User/Place/Reorder', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': csrfToken
                    },
                    body: JSON.stringify({ tripDayId: nd, orderedPlaceIds: newO })
                });

                const moved = tripPlaces.find(p => p.id === pid);
                if (moved) moved.tripDayId = nd;

                newO.forEach((id, idx) => {
                    const x = tripPlaces.find(p => p.id === id);
                    if (x) x.orderIndex = idx;
                });

                rebuildLegList(evt.from);
                rebuildLegList(evt.to);
                renderLegs();
            },

            onEnd: async evt => {
                if (evt.from === evt.to) {
                    const dayIdLocal = +evt.from.id.split('-').pop();
                    const ids = Array.from(evt.from.querySelectorAll('.place-item')).map(li => +li.dataset.id);
                    if (!dayIdLocal || ids.some(x => isNaN(x))) return;

                    await fetch('/User/Place/Reorder', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': csrfToken
                        },
                        body: JSON.stringify({ tripDayId: dayIdLocal, orderedPlaceIds: ids })
                    });

                    ids.forEach((id, idx) => {
                        const x = tripPlaces.find(p => p.id === id);
                        if (x) x.orderIndex = idx;
                    });

                    rebuildLegList(evt.from);
                    renderLegs();
                }
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const setupToggle = (displayId, formId, cancelBtnId, inputName = null) => {
        const display = document.getElementById(displayId);
        const form = document.getElementById(formId);
        const cancelBtn = document.getElementById(cancelBtnId);

        if (!display || !form || !cancelBtn) return;

        display.addEventListener('click', () => {
            display.classList.add('d-none');
            form.classList.remove('d-none');
            if (inputName) form.querySelector(`input[name="${inputName}"]`)?.focus();
        });

        cancelBtn.addEventListener('click', () => {
            form.classList.add('d-none');
            display.classList.remove('d-none');
        });
    };

    setupToggle('nameDisplay', 'nameEditForm', 'cancelNameEdit', 'newName');
    setupToggle('dateDisplay', 'dateEditForm', 'cancelDateEdit');
    setupToggle('checkinDisplay', 'timesEditForm', 'cancelTimesEdit');
    setupToggle('checkoutDisplay', 'timesEditForm', 'cancelTimesEdit');
});

window.initTripDetail = function () {
    try {
        window.tripPlaces = JSON.parse(window._tripPlacesJson);
        window.accom = (window._accomJson == null || window._accomJson === "null") ? null : JSON.parse(window._accomJson);
        window.tripDaysList = JSON.parse(window._tripDaysListJson);
    } catch (err) {
        console.error("Chyba při parsování JSON dat z Razor view:", err);
        return;
    }
    const destinationLat = parseFloat(window._destinationLat);
    const destinationLng = parseFloat(window._destinationLng);

    let map;
    let directionsService;
    let infoWindow;
    let currentTravelMode;
    const directionsRenderers = {};
    const colors = {};
    const palette = [
        "#e6194b", "#3cb44b", "#ffe119", "#4363d8", "#f58231",
        "#911eb4", "#46f0f0", "#f032e6", "#bcf60c", "#fabebe",
        "#008080", "#e6beff", "#9a6324", "#fffac8", "#800000"
    ];

    // Inicializace přepínače módu dopravy (Walking/Driving/Transit)
    currentTravelMode = google.maps.TravelMode.WALKING;
    document.querySelectorAll('#travelModeSwitcher .btn').forEach(btn => {
        btn.addEventListener('click', e => {
            document.querySelectorAll('#travelModeSwitcher .btn')
                .forEach(b => b.classList.remove('active'));
            e.currentTarget.classList.add('active');
            currentTravelMode = google.maps.TravelMode[e.currentTarget.dataset.mode];

            // po změně módu znovu sestavíme leg‐info a přepočteme trasy
            document.querySelectorAll('ul[id^="places-day-"]').forEach(ul => rebuildLegList(ul));
            renderLegs();
        });
    });

    // Autocomplete pro dny i ubytování
    initDayAutocomplete(destinationLat, destinationLng);
    initAccomodationAutocomplete(destinationLat, destinationLng);

    // InfoWindow (popup po kliknutí na mapě)
    infoWindow = new google.maps.InfoWindow();

    // Vykreslení mapy + markerů + DirectionsRenderer
    initMap();

    // Kliknutí na POI v mapě
    map.addListener('click', handlePlaceClick);

    // Po chvilce vygenerujeme a vykreslíme trasy pro všechny dny
    document.querySelectorAll('ul[id^="places-day-"]').forEach(ul => rebuildLegList(ul));
    setTimeout(() => renderLegs(), 100);

    initSortable();

    function initMap() {
        const center = { lat: destinationLat, lng: destinationLng };
        map = new google.maps.Map(document.getElementById("map"), {
            center,
            zoom: 13,
            clickableIcons: true
        });

        // 1) Vykreslíme všechny markery pro již existující místa
        let idx = 0;
        window.tripPlaces.forEach(p => {
            if (!colors[p.tripDayId]) {
                colors[p.tripDayId] = palette[idx++ % palette.length];
            }
            new google.maps.Marker({
                map,
                position: { lat: p.lat, lng: p.lng },
                title: p.name,
                icon: {
                    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                    scale: 4,
                    strokeColor: colors[p.tripDayId],
                    fillColor: colors[p.tripDayId],
                    fillOpacity: 1
                }
            });
        });

        // 2) Marker pro ubytování, pokud existuje
        if (window.accom) {
            new google.maps.Marker({
                map,
                position: { lat: window.accom.lat, lng: window.accom.lng },
                title: window.accom.name,
                icon: {
                    url: 'https://maps.google.com/mapfiles/ms/icons/lodging.png',
                    scaledSize: new google.maps.Size(32, 32)
                }
            });
        }

        // 3) Inicializujeme trasy pro každý den
        directionsService = new google.maps.DirectionsService();
        window.tripDaysList.forEach(day => {
            directionsRenderers[day.id] = new google.maps.DirectionsRenderer({
                map: map,
                suppressMarkers: true,
                preserveViewport: true,
                polylineOptions: {
                    strokeColor: colors[day.id] || palette.shift(),
                    strokeOpacity: 0.6,
                    strokeWeight: 4
                }
            });
        });
    }

    function renderLegs() {
        // 1) Odstranění předchozích linek
        if (window._transitPolylines) {
            window._transitPolylines.forEach(pl => pl.setMap(null));
        }
        window._transitPolylines = [];

        // 2) Každý den zvlášť
        window.tripDaysList.forEach(day => {
            const elems = document.querySelectorAll(`#places-day-${day.id} .leg-info`);
            const places = window.tripPlaces
                .filter(p => p.tripDayId === day.id)
                .sort((a, b) => a.orderIndex - b.orderIndex);

            if (places.length < 2) {
                directionsRenderers[day.id].setDirections({ routes: [] });
                elems.forEach(li => {
                    li.querySelector('.leg-text').textContent = '';
                    li.querySelector('.leg-directions').classList.add('d-none');
                });
                return;
            }

            if (currentTravelMode === google.maps.TravelMode.TRANSIT) {
                directionsRenderers[day.id].set('directions', null);

                for (let i = 0; i < places.length - 1; i++) {
                    const o = new google.maps.LatLng(places[i].lat, places[i].lng);
                    const d = new google.maps.LatLng(places[i + 1].lat, places[i + 1].lng);
                    const legReq = {
                        origin: o,
                        destination: d,
                        travelMode: google.maps.TravelMode.TRANSIT,
                        transitOptions: { departureTime: new Date() },
                        unitSystem: google.maps.UnitSystem.METRIC
                    };
                    directionsService.route(legReq, (result, status) => {
                        if (status === 'OK') {
                            const path = result.routes[0].overview_path;
                            const pl = new google.maps.Polyline({
                                path,
                                map,
                                strokeColor: colors[day.id],
                                strokeOpacity: 0.6,
                                strokeWeight: 4,
                                preserveViewport: true
                            });
                            window._transitPolylines.push(pl);

                            const leg = result.routes[0].legs[0];
                            const li = elems[i];
                            li.querySelector('.leg-text').textContent = `${leg.duration.text} · ${leg.distance.text}`;
                            const a = li.querySelector('.leg-directions');
                            a.href =
                                `https://www.google.com/maps/dir/?api=1` +
                                `&origin=${leg.start_location.lat()},${leg.start_location.lng()}` +
                                `&destination=${leg.end_location.lat()},${leg.end_location.lng()}` +
                                `&travelmode=transit`;
                            a.classList.remove('d-none');
                        }
                    });
                }

            } else {
                // Walking/Driving – jedním requestem s waypointy
                const origin = new google.maps.LatLng(places[0].lat, places[0].lng);
                const destination = new google.maps.LatLng(places.at(-1).lat, places.at(-1).lng);
                const waypoints = places.slice(1, -1).map(p => ({
                    location: new google.maps.LatLng(p.lat, p.lng),
                    stopover: true
                }));

                const req = {
                    origin,
                    destination,
                    travelMode: currentTravelMode,
                    waypoints,
                    unitSystem: google.maps.UnitSystem.METRIC
                };
                directionsService.route(req, (result, status) => {
                    if (status === 'OK') {
                        directionsRenderers[day.id].setDirections(result);

                        result.routes[0].legs.forEach((leg, i) => {
                            const li = elems[i];
                            li.querySelector('.leg-text').textContent = `${leg.duration.text} · ${leg.distance.text}`;
                            const a = li.querySelector('.leg-directions');
                            a.href =
                                `https://www.google.com/maps/dir/?api=1` +
                                `&origin=${leg.start_location.lat()},${leg.start_location.lng()}` +
                                `&destination=${leg.end_location.lat()},${leg.end_location.lng()}` +
                                `&travelmode=${currentTravelMode.toLowerCase()}`;
                            a.classList.remove('d-none');
                        });
                    }
                });
            }
        });
    }

    function rebuildLegList(ul) {
        ul.querySelectorAll('.leg-info').forEach(e => e.remove());
        const items = Array.from(ul.querySelectorAll('.place-item'));
        ul.innerHTML = '';
        items.forEach((li, i) => {
            ul.appendChild(li);
            if (i < items.length - 1) {
                let icon = 'bi-question-circle';
                switch (currentTravelMode) {
                    case google.maps.TravelMode.WALKING: icon = 'bi-person-walking'; break;
                    case google.maps.TravelMode.DRIVING: icon = 'bi-car-front'; break;
                    case google.maps.TravelMode.TRANSIT: icon = 'bi-bus-front'; break;
                }
                const li2 = document.createElement('li');
                li2.className = 'list-group-item leg-info small text-muted';
                li2.innerHTML = `
                    <i class="bi ${icon} me-1"></i>
                    <span class="leg-text">Načítám…</span>
                    <a href="#" class="ms-auto leg-directions d-none" target="_blank" rel="noopener noreferrer">Zobrazit trasu</a>
                `;
                ul.appendChild(li2);
            }
        });
    }

    function initDayAutocomplete(destLat, destLng) {
        const bias = new google.maps.Circle({ center: { lat: destLat, lng: destLng }, radius: 50000 });
        document.querySelectorAll(".google-place-input").forEach(input => {
            const ac = new google.maps.places.Autocomplete(input, {
                fields: ["place_id", "name", "geometry"],
                bounds: bias.getBounds(),
                strictBounds: false
            });
            ac.addListener("place_changed", () => {
                const plc = ac.getPlace();
                if (!plc.place_id || !plc.geometry) return;
                const dayId = +input.dataset.tripDayId;
                fetch('/User/Place/AddToDay', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({
                        tripDayId: dayId,
                        googlePlaceId: plc.place_id,
                        name: plc.name,
                        latitude: plc.geometry.location.lat(),
                        longitude: plc.geometry.location.lng()
                    })
                }).then(r => r.ok && location.reload());
            });
        });
    }

    function initAccomodationAutocomplete(destLat, destLng) {
        // destLat a destLng se předávají z initTripDetail, ale dále je neužijeme – 
        // můžete je klidně nechat nebo nahradit přímo window._destinationLat/… 
        const bias = new google.maps.Circle({
            center: { lat: destLat, lng: destLng },
            radius: 50000
        });

        const input = document.getElementById('accomodationInput');
        if (!input) return;

        const ac = new google.maps.places.Autocomplete(input, {
            fields: ['place_id', 'name', 'geometry'],
            bounds: bias.getBounds(),
            strictBounds: false
        });

        ac.addListener('place_changed', () => {
            const plc = ac.getPlace();
            if (!plc.place_id || !plc.geometry) return;
            const realTripId = parseInt(window._currentTripId, 10);

            fetch('/User/Accomodation/SetAccomodation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    tripId: realTripId,
                    googlePlaceId: plc.place_id,
                    name: plc.name,
                    latitude: plc.geometry.location.lat(),
                    longitude: plc.geometry.location.lng()
                })
            })
                .then(r => {
                    if (r.ok) {
                        location.reload();
                    } else {
                        console.error('Chyba při ukládání ubytování:', r.status, r.statusText);
                    }
                });
        });
    }

    function handlePlaceClick(e) {
        if (!e.placeId) return;
        e.stop();

        // Helper pro ochranu před XSS
        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }

        const svc = new google.maps.places.PlacesService(map);
        svc.getDetails({
            placeId: e.placeId,
            fields: ['place_id', 'name', 'formatted_address', 'geometry', 'rating', 'website']
        }, (place, status) => {
            if (status !== google.maps.places.PlacesServiceStatus.OK) return;

            let html = `<div style="max-width:280px">
            <h5>${escapeHtml(place.name)}</h5>
            <p class="small mb-1">${escapeHtml(place.formatted_address)}</p>`;

            if (place.rating) {
                html += `<p class="small mb-2">Hodnocení: ${escapeHtml(place.rating.toString())} ⭐</p>`;
            }

            html += `<select id="addPlaceDaySelect" class="form-select form-select-sm mb-2">
            ${window.tripDaysList.map(d => `<option value="${d.id}">${escapeHtml(d.display)}</option>`).join('')}
            </select>
            <button id="addPlaceBtn" class="btn btn-sm btn-primary w-100">Přidat do dne</button>
            </div>`;

            infoWindow.setContent(html);
            infoWindow.setPosition(place.geometry.location);
            infoWindow.open(map);

            google.maps.event.addListenerOnce(infoWindow, 'domready', () => {
                document.getElementById('addPlaceBtn').addEventListener('click', () => {
                    const dayId = +document.getElementById('addPlaceDaySelect').value;
                    fetch('/User/Place/AddToDay', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                        },
                        body: JSON.stringify({
                            tripDayId: dayId,
                            googlePlaceId: place.place_id,
                            name: place.name,
                            latitude: place.geometry.location.lat(),
                            longitude: place.geometry.location.lng()
                        })
                    }).then(r => r.ok && location.reload());
                });
            });
        });
    }


    window.rebuildLegList = rebuildLegList;
    window.renderLegs = renderLegs;
};
