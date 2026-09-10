(function () {
    "use strict";

    var apiBase = "/DinkFlixWatchLater";
    var currentCategory = "movie";
    var items = [];

    function getApiClient() {
        return (window.parent && window.parent.ApiClient) ? window.parent.ApiClient : window.ApiClient;
    }

    function authHeaders() {
        var client = getApiClient();
        return client ? { "X-Emby-Token": client.accessToken() } : {};
    }

    function getUserId() {
        var client = getApiClient();
        return client ? client.getCurrentUserId() : null;
    }

    function tmdbImage(path, size) {
        return path ? "https://image.tmdb.org/t/p/" + size + path : "";
    }

    function fetchItems() {
        var userId = getUserId();
        if (!userId) {
            return Promise.resolve([]);
        }
        return fetch(apiBase + "/items?userId=" + encodeURIComponent(userId), { headers: authHeaders() })
            .then(function (res) { return res.ok ? res.json() : []; });
    }

    function checkAvailability(item) {
        return fetch(apiBase + "/availability/" + item.tmdbId + "?mediaType=" + item.mediaType, { headers: authHeaders() })
            .then(function (res) { return res.ok ? res.json() : null; });
    }

    function removeItem(item) {
        var userId = getUserId();
        return fetch(apiBase + "/items/" + item.tmdbId + "?userId=" + encodeURIComponent(userId) + "&mediaType=" + item.mediaType, {
            method: "DELETE",
            headers: authHeaders()
        });
    }

    function requestItem(item) {
        return fetch(apiBase + "/request/" + item.tmdbId, {
            method: "POST",
            headers: Object.assign({ "Content-Type": "application/json" }, authHeaders()),
            body: JSON.stringify({ mediaType: item.mediaType })
        });
    }

    function buildCard(item) {
        var card = document.createElement("article");
        card.className = "wl-card";

        var img = document.createElement("img");
        img.className = "wl-poster";
        img.src = tmdbImage(item.posterPath, "w342");
        img.alt = item.title;
        card.appendChild(img);

        var body = document.createElement("div");
        body.className = "wl-card-body";

        var title = document.createElement("div");
        title.className = "wl-card-title";
        title.textContent = item.title;
        body.appendChild(title);

        var added = document.createElement("div");
        added.className = "wl-card-added";
        added.textContent = "Added " + new Date(item.addedAtUtc).toLocaleDateString();
        body.appendChild(added);

        var actions = document.createElement("div");
        actions.className = "wl-card-actions";

        var primaryBtn = document.createElement("button");
        primaryBtn.className = "wl-btn wl-btn-request";
        primaryBtn.textContent = "Checking\u2026";
        primaryBtn.disabled = true;
        actions.appendChild(primaryBtn);

        var removeBtn = document.createElement("button");
        removeBtn.className = "wl-btn wl-btn-remove";
        removeBtn.textContent = "Remove";
        removeBtn.addEventListener("click", function () {
            removeItem(item).then(loadAndRender);
        });
        actions.appendChild(removeBtn);

        body.appendChild(actions);
        card.appendChild(body);

        checkAvailability(item).then(function (availability) {
            applyAvailability(primaryBtn, item, availability);
        });

        return card;
    }

    function applyAvailability(button, item, availability) {
        if (!availability) {
            button.textContent = "Unavailable";
            return;
        }

        if (availability.availableOnJellyfin) {
            button.textContent = "Play";
            button.className = "wl-btn wl-btn-play";
            button.disabled = false;
            button.addEventListener("click", function () {
                window.parent.location.href = "/web/#/details?id=" + availability.jellyfinItemId;
            });
            return;
        }

        if (availability.seerrStatus !== "unknown") {
            button.textContent = availability.seerrStatus.replace("_", " ");
            return;
        }

        button.textContent = "Request";
        button.disabled = false;
        button.addEventListener("click", function () {
            button.disabled = true;
            button.textContent = "Requesting\u2026";
            requestItem(item).then(function (res) {
                button.textContent = res.ok ? "Requested" : "Failed";
            });
        });
    }

    function render() {
        var grid = document.getElementById("wl-grid");
        var empty = document.getElementById("wl-empty");
        grid.innerHTML = "";

        var filtered = items.filter(function (item) { return item.category === currentCategory; });
        empty.hidden = filtered.length > 0;
        filtered.forEach(function (item) { grid.appendChild(buildCard(item)); });
    }

    function loadAndRender() {
        fetchItems().then(function (data) {
            items = data;
            render();
        });
    }

    document.getElementById("wl-tabs").addEventListener("click", function (event) {
        if (!event.target.classList.contains("wl-tab")) {
            return;
        }
        document.querySelectorAll(".wl-tab").forEach(function (tab) { tab.classList.remove("active"); });
        event.target.classList.add("active");
        currentCategory = event.target.getAttribute("data-category");
        render();
    });

    loadAndRender();
})();
