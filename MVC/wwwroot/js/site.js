// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Text vergleichbar machen: klein und ohne Akzente (Fermín -> fermin)
function normalizeText(text) {
    return text.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
}

// Squad-Suche: Zeilen ausblenden, deren Name nicht passt
const searchInput = document.querySelector(".squad-search-input");

if (searchInput) {
    searchInput.addEventListener("input", () => {
        const search = normalizeText(searchInput.value);

        document.querySelectorAll(".squad-row").forEach(row => {
            const name = normalizeText(row.querySelector(".squad-player-name").textContent);
            row.style.display = name.includes(search) ? "" : "none";
        });
    });
}

// Favoriten-Stern: ohne Neuladen der Seite umschalten
document.querySelectorAll("form[action$='ToggleFavourite']").forEach(form => {
    form.addEventListener("submit", async event => {
        event.preventDefault();

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: { "X-Requested-With": "fetch" }
        });

        // Nicht eingeloggt -> zum Login
        if (response.status === 401) {
            window.location.href = "/Home/Login";
            return;
        }

        const data = await response.json();
        const button = form.querySelector("button");

        if (button.classList.contains("squad-fav")) {
            // Squad: Stern umschalten
            button.textContent = data.isFavourite ? "★" : "☆";
            button.classList.toggle("is-fav", data.isFavourite);
        } else {
            // Favourites: Karte entfernen
            form.closest(".favourites-card").remove();
        }
    });
});
