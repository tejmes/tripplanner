(() => {
    const inp = document.getElementById('userSearch');
    const hid = document.getElementById('userId');
    const list = document.getElementById('suggestions');
    const btn = document.getElementById('btnAdd');
    const trip = document.querySelector('[data-trip-id]').dataset.tripId;

    inp.addEventListener('input', async () => {
        hid.value = '';
        btn.disabled = true;
        const term = inp.value.trim();
        if (term.length < 2) {
            list.innerHTML = '';
            return;
        }
        const resp = await fetch(`/User/Collaborator/SearchUsers?term=${encodeURIComponent(term)}&tripId=${trip}`);
        const items = await resp.json();
        list.innerHTML = items.map(u =>
            `<button type="button" class="list-group-item list-group-item-action" data-id="${u.userId}">
         ${u.userName} &lt;${u.email}&gt;
       </button>`
        ).join('');
    });

    list.addEventListener('click', e => {
        const btnItem = e.target.closest('button[data-id]');
        if (!btnItem) return;
        hid.value = btnItem.dataset.id;
        inp.value = btnItem.textContent;
        list.innerHTML = '';
        btn.disabled = false;
    });

    document.addEventListener('click', e => {
        if (!list.contains(e.target) && e.target !== inp) {
            list.innerHTML = '';
        }
    });
})();
