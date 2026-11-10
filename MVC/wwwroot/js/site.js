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

        // Anderer Fehler (z.B. Spieler wurde inzwischen geloescht) -> Seite neu laden, zeigt den aktuellen Stand
        if (!response.ok) {
            window.location.reload();
            return;
        }

        const data = await response.json();
        const button = form.querySelector("button");

        if (button.classList.contains("squad-fav")) {
            // Squad: Stern umschalten
            button.textContent = data.isFavourite ? "★" : "☆";
            button.classList.toggle("is-fav", data.isFavourite);
        } else {
            // Favourites: Karte entfernen und Zahl im Banner anpassen
            form.closest(".favourites-card").remove();

            const left = document.querySelectorAll(".favourites-card").length;
            if (left === 0) {
                // Letzter Favorit weg -> Seite neu laden, zeigt den leeren Zustand
                window.location.reload();
                return;
            }
            const count = document.querySelector(".favourites-count");
            if (count) {
                count.textContent = left;
                count.nextSibling.textContent = left === 1 ? " player" : " players";
            }
        }
    });
});

// Admin: Formulare mit data-confirm erst nach Bestaetigung absenden (z.B. Loeschen)
document.querySelectorAll("form[data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
        if (!confirm(form.dataset.confirm)) {
            event.preventDefault();
        }
    });
});

// Favoriten als Story-Bild (1080x1920) zeichnen und herunterladen
const shareButton = document.querySelector(".favourites-share-btn");

if (shareButton) {
    shareButton.addEventListener("click", async () => {
        shareButton.disabled = true;
        try {
            const blob = await drawFavouritesStory(shareButton.dataset.username);
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = "my-barca-favourites.png";
            link.click();
            setTimeout(() => URL.revokeObjectURL(link.href), 1000);
        } catch {
            alert("The image could not be created. Please try again.");
        } finally {
            shareButton.disabled = false;
        }
    });
}

function loadImage(src) {
    return new Promise(resolve => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => resolve(null); // Fehlendes Foto -> Karte ohne Bild
        img.src = src;
    });
}

function roundRect(ctx, x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
}

// Schrift so lange verkleinern, bis der Text in die Breite passt
function fitFont(ctx, text, maxWidth, size, family) {
    do {
        ctx.font = `${size}px ${family}`;
        size -= 2;
    } while (ctx.measureText(text).width > maxWidth && size > 10);
}

async function drawFavouritesStory(username) {
    const W = 1080, H = 1920, PAD = 70, GAP = 28;
    const NAVY = "#0a1a4d", MAROON = "#7a0f3d", GOLD = "#edbb00";
    const DISPLAY = '"Anton", sans-serif', BODY = '"Barlow Condensed", sans-serif';

    // Aktuelle Karten lesen (entfernte Favoriten sind dann schon weg)
    const players = [...document.querySelectorAll(".favourites-card")].map(card => card.dataset);

    await Promise.all([
        document.fonts.load(`80px ${DISPLAY}`),
        document.fonts.load(`700 40px ${BODY}`)
    ]);
    const [logo, ...photos] = await Promise.all([
        loadImage("/images/logobarca.png"),
        ...players.map(p => loadImage(p.img))
    ]);

    const canvas = document.createElement("canvas");
    canvas.width = W;
    canvas.height = H;
    const ctx = canvas.getContext("2d");

    // Hintergrund: Blau oben, Granat unten
    const bg = ctx.createLinearGradient(0, 0, W * 0.4, H);
    bg.addColorStop(0, NAVY);
    bg.addColorStop(0.55, "#14246a");
    bg.addColorStop(1, MAROON);
    ctx.fillStyle = bg;
    ctx.fillRect(0, 0, W, H);

    // Kopf: Logo und Titel
    const logoH = 150;
    if (logo) ctx.drawImage(logo, (W - logoH * logo.width / logo.height) / 2, 90, logoH * logo.width / logo.height, logoH);

    ctx.textAlign = "center";
    ctx.fillStyle = GOLD;
    ctx.font = `96px ${DISPLAY}`;
    ctx.fillText("MY FAVOURITES", W / 2, 350);
    ctx.fillStyle = "rgba(255,255,255,0.75)";
    ctx.font = `600 38px ${BODY}`;
    ctx.fillText(`FC BARCELONA · SEASON 2026/27 · ${players.length} ${players.length === 1 ? "PLAYER" : "PLAYERS"}`, W / 2, 410);

    // Raster: so wenige Spalten wie moeglich, damit die Karten gross bleiben
    const top = 470, bottom = H - 170, availH = bottom - top, availW = W - PAD * 2;
    let cols = players.length <= 4 ? 2 : 3, cellW, cellH, rows;
    while (true) {
        cellW = (availW - GAP * (cols - 1)) / cols;
        cellH = cellW * 1.32;
        rows = Math.ceil(players.length / cols);
        if (rows * cellH + (rows - 1) * GAP <= availH || cols >= 6) break;
        cols++;
    }
    // Passt es auch mit 6 Spalten nicht, Karten flacher machen
    if (rows * cellH + (rows - 1) * GAP > availH) cellH = (availH - GAP * (rows - 1)) / rows;

    const gridH = rows * cellH + (rows - 1) * GAP;
    const gridTop = top + (availH - gridH) / 2;

    players.forEach((p, i) => {
        const row = Math.floor(i / cols);
        const inRow = Math.min(cols, players.length - row * cols); // letzte Reihe zentrieren
        const col = i % cols;
        const x = PAD + (availW - (inRow * cellW + (inRow - 1) * GAP)) / 2 + col * (cellW + GAP);
        const y = gridTop + row * (cellH + GAP);
        const photoH = cellH * 0.72;
        const r = cellW * 0.08;

        // Karte
        ctx.save();
        roundRect(ctx, x, y, cellW, cellH, r);
        ctx.clip();
        const cardBg = ctx.createLinearGradient(0, y, 0, y + cellH);
        cardBg.addColorStop(0, "#1d2f7a");
        cardBg.addColorStop(1, "#0d1a4f");
        ctx.fillStyle = cardBg;
        ctx.fillRect(x, y, cellW, cellH);

        // Foto: Bereich ausfuellen, oben ausrichten
        const photo = photos[i];
        if (photo) {
            const scale = Math.max(cellW / photo.width, photoH / photo.height);
            const pw = photo.width * scale, ph = photo.height * scale;
            ctx.drawImage(photo, x + (cellW - pw) / 2, y, pw, ph);
        }
        // Weicher Uebergang vom Foto in den Textbereich
        const fade = ctx.createLinearGradient(0, y + photoH * 0.55, 0, y + photoH);
        fade.addColorStop(0, "rgba(13,26,79,0)");
        fade.addColorStop(1, "#14236a");
        ctx.fillStyle = fade;
        ctx.fillRect(x, y + photoH * 0.55, cellW, photoH * 0.45 + 1);
        ctx.fillStyle = "#14236a";
        ctx.fillRect(x, y + photoH, cellW, cellH - photoH);
        ctx.restore();

        // Rueckennummer oben links
        ctx.textAlign = "left";
        ctx.fillStyle = GOLD;
        ctx.font = `${Math.round(cellW * 0.2)}px ${DISPLAY}`;
        ctx.fillText(p.number || "", x + cellW * 0.08, y + cellW * 0.24);

        // Name und Position
        ctx.textAlign = "center";
        ctx.fillStyle = "#ffffff";
        fitFont(ctx, p.name.toUpperCase(), cellW * 0.88, Math.round(cellW * 0.13), DISPLAY);
        ctx.fillText(p.name.toUpperCase(), x + cellW / 2, y + photoH + (cellH - photoH) * 0.5);
        ctx.fillStyle = GOLD;
        ctx.font = `700 ${Math.round(cellW * 0.085)}px ${BODY}`;
        ctx.fillText(p.pos || "", x + cellW / 2, y + photoH + (cellH - photoH) * 0.84);
    });

    // Fuss: wer das Bild gemacht hat
    ctx.textAlign = "center";
    ctx.fillStyle = "#ffffff";
    ctx.font = `700 44px ${BODY}`;
    ctx.fillText(username ? `@${username}` : "Visca Barça", W / 2, H - 95);
    ctx.fillStyle = "rgba(255,255,255,0.6)";
    ctx.font = `500 30px ${BODY}`;
    ctx.fillText("Més que un club", W / 2, H - 50);

    return new Promise((resolve, reject) =>
        canvas.toBlob(blob => blob ? resolve(blob) : reject(), "image/png"));
}
