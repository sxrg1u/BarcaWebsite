// Aufstellung: Spieler per Drag & Drop (Maus und Touch) oder per Antippen auf Positionen setzen.
// Nutzt loadImage, roundRect und fitFont aus site.js fuer den Bild-Export.

(() => {
    const root = document.querySelector(".lineup");
    if (!root) return;

    const STARTERS = 11, BENCH = 9, SLOTS = STARTERS + BENCH;
    const DRAFT_KEY = "barca-lineup-draft";

    // Positionen auf dem Feld in Prozent (x von links, y von oben; oben wird angegriffen).
    // Reihenfolge immer: Torwart, Abwehr, Mittelfeld, Sturm -> beim Systemwechsel bleiben Spieler grob an ihrem Platz
    const BACK4 = [[14, 72, "LB"], [37, 76, "CB"], [63, 76, "CB"], [86, 72, "RB"]];
    const BACK3 = [[27, 75, "CB"], [50, 76, "CB"], [73, 75, "CB"]];
    const GK = [[50, 92, "GK"]];
    const FORMATIONS = {
        "4-3-3":     [...GK, ...BACK4, [28, 49, "CM"], [50, 59, "DM"], [72, 49, "CM"], [17, 25, "LW"], [50, 18, "ST"], [83, 25, "RW"]],
        "4-2-3-1":   [...GK, ...BACK4, [36, 59, "DM"], [64, 59, "DM"], [16, 37, "LW"], [50, 39, "AM"], [84, 37, "RW"], [50, 17, "ST"]],
        "4-4-2":     [...GK, ...BACK4, [13, 48, "LM"], [38, 53, "CM"], [62, 53, "CM"], [87, 48, "RM"], [37, 21, "ST"], [63, 21, "ST"]],
        "4-1-2-1-2": [...GK, ...BACK4, [50, 60, "DM"], [27, 49, "CM"], [73, 49, "CM"], [50, 37, "AM"], [37, 19, "ST"], [63, 19, "ST"]],
        "3-5-2":     [...GK, ...BACK3, [11, 50, "LWB"], [31, 52, "CM"], [50, 59, "DM"], [69, 52, "CM"], [89, 50, "RWB"], [37, 21, "ST"], [63, 21, "ST"]],
        "3-4-3":     [...GK, ...BACK3, [13, 50, "LM"], [38, 55, "CM"], [62, 55, "CM"], [87, 50, "RM"], [18, 25, "LW"], [50, 18, "ST"], [82, 25, "RW"]],
        "5-3-2":     [...GK, [9, 63, "LWB"], [29, 74, "CB"], [50, 76, "CB"], [71, 74, "CB"], [91, 63, "RWB"], [27, 48, "CM"], [50, 53, "CM"], [73, 48, "CM"], [37, 20, "ST"], [63, 20, "ST"]]
    };

    const GROUPS = { GK: "GK", LB: "DEF", CB: "DEF", RB: "DEF", DM: "MID", CM: "MID", AM: "MID", LW: "FWD", RW: "FWD", ST: "FWD" };

    const pitch = root.querySelector(".lu-pitch");
    const bench = root.querySelector(".lu-bench");
    const pool = root.querySelector(".lu-pool");
    const formationSelect = root.querySelector(".lu-formation");
    const captainSelect = root.querySelector(".lu-captain");
    const statusEl = root.querySelector(".lu-status");
    const saveForm = root.querySelector(".lu-save-form");
    const nameInput = root.querySelector(".lu-name");
    const loggedIn = root.dataset.loggedIn === "true";
    const lineupId = Number(root.dataset.id) || 0; // 0 = neue, noch nicht gespeicherte Aufstellung

    // Alle Spieler aus der Liste (id -> Daten)
    const players = new Map();
    root.querySelectorAll(".lu-player").forEach(btn => players.set(Number(btn.dataset.id), btn.dataset));

    const state = {
        formation: "4-3-3",
        slots: new Array(SLOTS).fill(0),
        captain: null,
        selected: null // { type: "pool", id } oder { type: "slot", index }
    };

    // ---------- Start: gespeicherte Aufstellung oder Entwurf laden ----------

    function readDraft() {
        try { return JSON.parse(localStorage.getItem(DRAFT_KEY)); } catch { return null; }
    }

    // Entwurf nur fuer neue Aufstellungen merken (gespeicherte liegen in der DB)
    function writeDraft() {
        if (lineupId) return;
        try {
            localStorage.setItem(DRAFT_KEY, JSON.stringify({
                name: nameInput?.value ?? "", formation: state.formation, slots: state.slots, captain: state.captain
            }));
        } catch { /* ohne Speicher geht es auch */ }
    }

    function clearDraft() {
        try { localStorage.removeItem(DRAFT_KEY); } catch { }
    }

    function applyData(data) {
        if (!data) return;
        if (FORMATIONS[data.formation]) state.formation = data.formation;
        const seen = new Set();
        state.slots = new Array(SLOTS).fill(0).map((_, i) => {
            const id = Number(data.slots?.[i]) || 0;
            // Nur existierende Spieler, keiner doppelt
            if (!players.has(id) || seen.has(id)) return 0;
            seen.add(id);
            return id;
        });
        const captain = Number(data.captain) || null;
        state.captain = state.slots.slice(0, STARTERS).includes(captain) ? captain : null;
    }

    const server = {
        formation: root.dataset.formation,
        slots: (root.dataset.slots || "").split(",").map(Number),
        captain: root.dataset.captain
    };
    const params = new URLSearchParams(location.search);
    let dirty = false;

    if (lineupId) {
        // Gespeicherte Aufstellung aus der DB
        applyData(server);
    } else if (params.has("new")) {
        // "+ New lineup": leer anfangen
        clearDraft();
        history.replaceState(null, "", location.pathname);
    } else {
        // Entwurf aus dem Browser, z.B. als Gast gebaut und danach eingeloggt
        const draft = readDraft();
        if (draft) {
            applyData(draft);
            if (nameInput && draft.name) nameInput.value = draft.name;
            dirty = loggedIn && state.slots.some(Boolean);
        }
    }

    nameInput?.addEventListener("input", () => {
        dirty = true;
        writeDraft();
        render();
    });

    // ---------- Darstellung ----------

    function createToken(id, index) {
        const p = players.get(id);
        const token = document.createElement("div");
        token.className = "lu-token";
        token.dataset.id = id;

        const img = document.createElement("img");
        img.src = p.img;
        img.alt = "";
        img.draggable = false;
        token.append(img);

        if (p.number) {
            const no = document.createElement("span");
            no.className = "lu-token-no";
            no.textContent = p.number;
            token.append(no);
        }

        if (state.captain === id && index < STARTERS) {
            const c = document.createElement("span");
            c.className = "lu-token-captain";
            c.textContent = "C";
            c.title = "Captain";
            token.append(c);
        }

        const name = document.createElement("span");
        name.className = "lu-token-name";
        name.textContent = shortName(p.name);
        token.append(name);

        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "lu-token-remove";
        remove.setAttribute("aria-label", `Remove ${p.name}`);
        remove.textContent = "×";
        token.append(remove);

        return token;
    }

    // "Robert Lewandowski" -> "Lewandowski", "Lamine Yamal" -> "Yamal"
    function shortName(name) {
        const parts = name.trim().split(/\s+/);
        return parts.length > 1 ? parts.slice(1).join(" ") : name;
    }

    function createSlot(index, label) {
        const slot = document.createElement("div");
        slot.className = "lu-slot";
        slot.dataset.index = index;
        slot.tabIndex = 0;
        slot.setAttribute("role", "button");

        const id = state.slots[index];
        if (id) {
            slot.classList.add("is-filled");
            slot.append(createToken(id, index));
            slot.setAttribute("aria-label", `${label}: ${players.get(id).name}`);
        } else {
            const empty = document.createElement("span");
            empty.className = "lu-slot-empty";
            empty.textContent = label;
            slot.append(empty);
            slot.setAttribute("aria-label", `${label}: empty`);
        }

        if (state.selected?.type === "slot" && state.selected.index === index) slot.classList.add("is-selected");
        if (state.selected) slot.classList.add("is-target");
        return slot;
    }

    function render() {
        // Feld
        pitch.querySelectorAll(".lu-slot").forEach(el => el.remove());
        FORMATIONS[state.formation].forEach(([x, y, label], i) => {
            const slot = createSlot(i, label);
            slot.style.left = `${x}%`;
            slot.style.top = `${y}%`;
            pitch.append(slot);
        });

        // Bank
        bench.replaceChildren(...Array.from({ length: BENCH }, (_, i) => createSlot(STARTERS + i, "SUB")));

        // Liste: aufgestellte Spieler ausgrauen
        const used = new Set(state.slots.filter(Boolean));
        root.querySelectorAll(".lu-player").forEach(btn => {
            const id = Number(btn.dataset.id);
            btn.classList.toggle("is-used", used.has(id));
            btn.classList.toggle("is-selected", state.selected?.type === "pool" && state.selected.id === id);
        });

        // Kapitaen-Auswahl: nur Spieler der Startelf
        const starters = state.slots.slice(0, STARTERS).filter(Boolean);
        captainSelect.replaceChildren(new Option("-", ""), ...starters.map(id => {
            const opt = new Option(players.get(id).name, id);
            opt.selected = state.captain === id;
            return opt;
        }));

        formationSelect.value = state.formation;
        const count = starters.length;
        root.classList.toggle("has-selection", !!state.selected);
        setStatus(dirty && loggedIn ? `Unsaved changes · ${count}/11` : `${count}/11 players on the pitch`);
    }

    function setStatus(text, kind = "") {
        statusEl.textContent = text;
        statusEl.dataset.kind = kind;
    }

    function changed() {
        dirty = true;
        if (!state.slots.slice(0, STARTERS).includes(state.captain)) state.captain = null;
        writeDraft();
        render();
    }

    // ---------- Aktionen ----------

    // source: { type: "pool", id } oder { type: "slot", index }
    function place(source, target) {
        if (source.type === "pool") {
            const existing = state.slots.indexOf(source.id);
            if (existing >= 0) return place({ type: "slot", index: existing }, target);
            state.slots[target] = source.id; // wer vorher dort stand, geht zurueck in die Liste
        } else {
            if (source.index === target) return;
            [state.slots[source.index], state.slots[target]] = [state.slots[target], state.slots[source.index]];
        }
        state.selected = null;
        changed();
    }

    function removeAt(index) {
        state.slots[index] = 0;
        state.selected = null;
        changed();
    }

    formationSelect.addEventListener("change", () => {
        state.formation = formationSelect.value;
        changed();
    });

    captainSelect.addEventListener("change", () => {
        state.captain = Number(captainSelect.value) || null;
        changed();
    });

    root.querySelector(".lu-reset").addEventListener("click", () => {
        if (state.slots.some(Boolean) && !confirm("Remove all players from the lineup?")) return;
        state.slots.fill(0);
        state.captain = null;
        state.selected = null;
        changed();
    });

    // ---------- Antippen: Spieler waehlen, dann Platz waehlen ----------

    let suppressClick = false;

    root.addEventListener("click", event => {
        if (suppressClick) { suppressClick = false; return; }

        const remove = event.target.closest(".lu-token-remove");
        if (remove) {
            removeAt(Number(remove.closest(".lu-slot").dataset.index));
            return;
        }

        const playerBtn = event.target.closest(".lu-player");
        if (playerBtn) {
            const id = Number(playerBtn.dataset.id);
            const index = state.slots.indexOf(id);
            if (state.selected?.type === "slot" && index < 0) {
                // Platz war gewaehlt -> diesen Spieler dort einsetzen
                place({ type: "pool", id }, state.selected.index);
                return;
            }
            const same = state.selected?.type === "pool" && state.selected.id === id;
            state.selected = same || index >= 0 ? null : { type: "pool", id };
            render();
            return;
        }

        const slot = event.target.closest(".lu-slot");
        if (slot) {
            const index = Number(slot.dataset.index);
            if (state.selected) {
                if (state.selected.type === "slot" && state.selected.index === index) state.selected = null;
                else return place(state.selected, index);
            } else {
                // Leerer oder besetzter Platz waehlen: danach Spieler oder anderen Platz antippen
                state.selected = { type: "slot", index };
            }
            render();
            return;
        }

        // Daneben getippt -> Auswahl aufheben
        if (state.selected && !event.target.closest(".lu-toolbar, .lu-pool")) {
            state.selected = null;
            render();
        }
    });

    root.addEventListener("keydown", event => {
        if ((event.key === "Enter" || event.key === " ") && event.target.classList.contains("lu-slot")) {
            event.preventDefault();
            event.target.click();
        }
        if ((event.key === "Delete" || event.key === "Backspace") && event.target.classList.contains("lu-slot")) {
            const index = Number(event.target.dataset.index);
            if (state.slots[index]) removeAt(index);
        }
        if (event.key === "Escape" && state.selected) {
            state.selected = null;
            render();
        }
    });

    // ---------- Drag & Drop mit Pointer Events (Maus + Touch) ----------

    let drag = null;

    root.addEventListener("pointerdown", event => {
        if (event.button !== 0 || event.target.closest(".lu-token-remove")) return;

        const playerBtn = event.target.closest(".lu-player");
        const token = event.target.closest(".lu-slot.is-filled");
        let source, id;
        if (playerBtn && !playerBtn.classList.contains("is-used")) {
            id = Number(playerBtn.dataset.id);
            source = { type: "pool", id };
        } else if (token) {
            const index = Number(token.dataset.index);
            id = state.slots[index];
            source = { type: "slot", index };
        } else {
            return;
        }

        drag = { source, id, startX: event.clientX, startY: event.clientY, ghost: null, pointerId: event.pointerId, el: event.target };
    });

    window.addEventListener("pointermove", event => {
        if (!drag || event.pointerId !== drag.pointerId) return;

        if (!drag.ghost) {
            // Erst ab ein paar Pixeln Bewegung ist es ein Ziehen (sonst ein Antippen)
            if (Math.hypot(event.clientX - drag.startX, event.clientY - drag.startY) < 6) return;
            const p = players.get(drag.id);
            drag.ghost = document.createElement("div");
            drag.ghost.className = "lu-ghost";
            drag.ghost.innerHTML = `<img alt="" /><span></span>`;
            drag.ghost.querySelector("img").src = p.img;
            drag.ghost.querySelector("span").textContent = shortName(p.name);
            document.body.append(drag.ghost);
            root.classList.add("is-dragging");
            if (state.selected) {
                state.selected = null;
                render();
            }
        }

        event.preventDefault();
        drag.ghost.style.transform = `translate(${event.clientX}px, ${event.clientY}px)`;

        root.querySelectorAll(".lu-slot.is-over").forEach(el => el.classList.remove("is-over"));
        const over = document.elementFromPoint(event.clientX, event.clientY)?.closest(".lu-slot");
        over?.classList.add("is-over");
    }, { passive: false });

    function endDrag(event, cancelled) {
        if (!drag || event.pointerId !== drag.pointerId) return;
        const d = drag;
        drag = null;
        if (!d.ghost) return; // war nur ein Antippen -> click-Handler uebernimmt

        d.ghost.remove();
        root.classList.remove("is-dragging");
        suppressClick = true;
        setTimeout(() => { suppressClick = false; }, 0);
        if (cancelled) { render(); return; }

        const under = document.elementFromPoint(event.clientX, event.clientY);
        const target = under?.closest(".lu-slot");
        if (target) {
            place(d.source, Number(target.dataset.index));
        } else if (d.source.type === "slot" && !under?.closest(".lu-pitch, .lu-bench")) {
            // Vom Feld weggezogen (z.B. zurueck in die Liste) -> entfernen
            removeAt(d.source.index);
        } else {
            render();
        }
    }

    window.addEventListener("pointerup", event => endDrag(event, false));
    window.addEventListener("pointercancel", event => endDrag(event, true));

    // ---------- Liste filtern ----------

    const search = root.querySelector(".lu-search");
    let group = "ALL";

    function filterList() {
        const term = normalizeText(search.value);
        root.querySelectorAll(".lu-player").forEach(btn => {
            const matchGroup = group === "ALL" || GROUPS[btn.dataset.pos] === group;
            const matchName = normalizeText(btn.dataset.name).includes(term);
            btn.parentElement.hidden = !(matchGroup && matchName);
        });
    }

    search.addEventListener("input", filterList);
    root.querySelectorAll(".lu-filter").forEach(btn => btn.addEventListener("click", () => {
        group = btn.dataset.group;
        root.querySelectorAll(".lu-filter").forEach(b => b.classList.toggle("is-active", b === btn));
        filterList();
    }));

    // ---------- Speichern ----------

    const SAVED_FLAG = "barca-lineup-saved";

    // asNew = true -> als neue Aufstellung speichern, sonst die geladene ueberschreiben
    async function save(asNew) {
        const name = nameInput.value.trim();
        if (!name) {
            setStatus("Please give your lineup a name.", "error");
            nameInput.focus();
            return;
        }

        const buttons = root.querySelectorAll(".lu-save, .lu-save-new");
        buttons.forEach(b => b.disabled = true);

        const data = new FormData(saveForm);
        data.append("id", asNew ? 0 : lineupId);
        data.append("name", name);
        data.append("formation", state.formation);
        state.slots.forEach(id => data.append("slots", id));
        if (state.captain) data.append("captainId", state.captain);

        try {
            const response = await fetch(saveForm.action, {
                method: "POST",
                body: data,
                headers: { "X-Requested-With": "fetch" }
            });
            if (response.status === 401) {
                window.location.href = "/Home/Login?returnUrl=%2FHome%2FLineup";
                return;
            }
            const result = await response.json().catch(() => ({}));
            if (!response.ok) {
                setStatus(result.error || "Could not save. Please try again.", "error");
                return;
            }

            // Seite mit der gespeicherten Aufstellung neu laden -> Liste unten ist aktuell
            dirty = false;
            if (!lineupId || asNew) clearDraft();
            try { sessionStorage.setItem(SAVED_FLAG, "1"); } catch { }
            window.location.href = `/Home/Lineup?id=${result.id}`;
        } catch {
            setStatus("Could not save. Please try again.", "error");
        } finally {
            buttons.forEach(b => b.disabled = false);
        }
    }

    saveForm?.addEventListener("submit", event => {
        event.preventDefault();
        save(false);
    });

    root.querySelector(".lu-save-new")?.addEventListener("click", () => save(true));

    // Warnen, wenn man mit ungespeicherten Aenderungen die Seite verlaesst
    window.addEventListener("beforeunload", event => {
        if (dirty && loggedIn) event.preventDefault();
    });

    // ---------- Als Bild speichern ----------

    const imageButton = root.querySelector(".lu-image");
    imageButton.addEventListener("click", async () => {
        imageButton.disabled = true;
        try {
            const blob = await drawLineupImage(root.dataset.username);
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = `my-barca-lineup-${state.formation}.png`;
            link.click();
            setTimeout(() => URL.revokeObjectURL(link.href), 1000);
        } catch {
            alert("The image could not be created. Please try again.");
        } finally {
            imageButton.disabled = false;
        }
    });

    async function drawLineupImage(username) {
        const W = 1080, H = 1920;
        const NAVY = "#0a1a4d", MAROON = "#7a0f3d", GOLD = "#edbb00";
        const DISPLAY = '"Anton", sans-serif', BODY = '"Barlow Condensed", sans-serif';

        await Promise.all([document.fonts.load(`80px ${DISPLAY}`), document.fonts.load(`700 40px ${BODY}`)]);
        const ids = [...new Set(state.slots.filter(Boolean))];
        const [logo, ...photoList] = await Promise.all([
            loadImage("/images/logobarca.png"),
            ...ids.map(id => loadImage(players.get(id).img))
        ]);
        const photos = new Map(ids.map((id, i) => [id, photoList[i]]));

        const canvas = document.createElement("canvas");
        canvas.width = W;
        canvas.height = H;
        const ctx = canvas.getContext("2d");

        // Hintergrund wie beim Favoriten-Bild
        const bg = ctx.createLinearGradient(0, 0, W * 0.4, H);
        bg.addColorStop(0, NAVY);
        bg.addColorStop(0.55, "#14246a");
        bg.addColorStop(1, MAROON);
        ctx.fillStyle = bg;
        ctx.fillRect(0, 0, W, H);

        // Kopf
        const logoH = 110;
        if (logo) ctx.drawImage(logo, (W - logoH * logo.width / logo.height) / 2, 60, logoH * logo.width / logo.height, logoH);
        ctx.textAlign = "center";
        ctx.fillStyle = GOLD;
        ctx.font = `88px ${DISPLAY}`;
        // Titel: Name der Aufstellung, sonst Standardtext
        const title = (nameInput?.value.trim() || "My starting XI").toUpperCase();
        fitFont(ctx, title, W - 140, 88, DISPLAY);
        ctx.fillText(title, W / 2, 268);
        ctx.fillStyle = "rgba(255,255,255,0.75)";
        ctx.font = `600 36px ${BODY}`;
        ctx.fillText(`FC BARCELONA · SEASON 2026/27 · ${state.formation}`, W / 2, 322);

        // Spielfeld
        const fh = 1130, fw = fh * 100 / 140;
        const fx = (W - fw) / 2, fy = 360;
        ctx.save();
        roundRect(ctx, fx, fy, fw, fh, 28);
        ctx.clip();
        for (let i = 0; i < 10; i++) {
            ctx.fillStyle = i % 2 ? "#2f7d3e" : "#348a45";
            ctx.fillRect(fx, fy + i * fh / 10, fw, fh / 10 + 1);
        }
        ctx.restore();

        const sx = fw / 100, sy = fh / 140;
        ctx.strokeStyle = "rgba(255,255,255,0.55)";
        ctx.lineWidth = 4;
        const lineRect = (x, y, w, h) => ctx.strokeRect(fx + x * sx, fy + y * sy, w * sx, h * sy);
        lineRect(3, 3, 94, 134);
        lineRect(24, 3, 52, 20);
        lineRect(38, 3, 24, 7);
        lineRect(24, 117, 52, 20);
        lineRect(38, 130, 24, 7);
        ctx.beginPath();
        ctx.moveTo(fx + 3 * sx, fy + 70 * sy);
        ctx.lineTo(fx + 97 * sx, fy + 70 * sy);
        ctx.stroke();
        ctx.beginPath();
        ctx.arc(fx + 50 * sx, fy + 70 * sy, 11 * sx, 0, Math.PI * 2);
        ctx.stroke();

        // Ein Spieler: rundes Foto, Nummer, Name
        function drawPlayer(id, cx, cy, r, label) {
            if (!id) {
                ctx.setLineDash([10, 8]);
                ctx.strokeStyle = "rgba(255,255,255,0.6)";
                ctx.lineWidth = 3;
                ctx.beginPath();
                ctx.arc(cx, cy, r, 0, Math.PI * 2);
                ctx.stroke();
                ctx.setLineDash([]);
                ctx.fillStyle = "rgba(255,255,255,0.7)";
                ctx.font = `700 ${Math.round(r * 0.5)}px ${BODY}`;
                ctx.textAlign = "center";
                ctx.fillText(label, cx, cy + r * 0.18);
                return;
            }
            const p = players.get(id);

            ctx.save();
            ctx.beginPath();
            ctx.arc(cx, cy, r, 0, Math.PI * 2);
            ctx.fillStyle = "#1d2f7a";
            ctx.fill();
            ctx.clip();
            const photo = photos.get(id);
            if (photo) {
                const scale = Math.max((r * 2) / photo.width, (r * 2) / photo.height);
                ctx.drawImage(photo, cx - photo.width * scale / 2, cy - r, photo.width * scale, photo.height * scale);
            }
            ctx.restore();
            ctx.strokeStyle = GOLD;
            ctx.lineWidth = Math.max(3, r * 0.07);
            ctx.beginPath();
            ctx.arc(cx, cy, r, 0, Math.PI * 2);
            ctx.stroke();

            // Rueckennummer
            if (p.number) {
                const nr = r * 0.36;
                ctx.fillStyle = NAVY;
                ctx.beginPath();
                ctx.arc(cx - r * 0.78, cy - r * 0.7, nr, 0, Math.PI * 2);
                ctx.fill();
                ctx.fillStyle = GOLD;
                ctx.font = `${Math.round(nr * 1.15)}px ${DISPLAY}`;
                ctx.textAlign = "center";
                ctx.fillText(p.number, cx - r * 0.78, cy - r * 0.7 + nr * 0.42);
            }

            // Kapitaensbinde
            if (id === state.captain && state.slots.indexOf(id) < STARTERS) {
                const cr = r * 0.34;
                ctx.fillStyle = GOLD;
                ctx.beginPath();
                ctx.arc(cx + r * 0.78, cy - r * 0.7, cr, 0, Math.PI * 2);
                ctx.fill();
                ctx.fillStyle = NAVY;
                ctx.font = `${Math.round(cr * 1.2)}px ${DISPLAY}`;
                ctx.fillText("C", cx + r * 0.78, cy - r * 0.7 + cr * 0.42);
            }

            // Name auf dunklem Band
            // Anton kennt Buchstaben wie "Ę" nicht -> nur fuer solche den Grundbuchstaben nehmen
            const name = [...shortName(p.name).toUpperCase()]
                .map(ch => ch.charCodeAt(0) > 0xff ? ch.normalize("NFD")[0] : ch).join("");
            const maxW = r * 3;
            fitFont(ctx, name, maxW - 16, Math.round(r * 0.42), DISPLAY);
            const tw = Math.min(ctx.measureText(name).width + 22, maxW);
            const th = r * 0.55;
            ctx.fillStyle = "rgba(10,26,77,0.88)";
            roundRect(ctx, cx - tw / 2, cy + r + 8, tw, th, th / 2);
            ctx.fill();
            ctx.fillStyle = "#ffffff";
            ctx.textAlign = "center";
            ctx.fillText(name, cx, cy + r + 8 + th * 0.76);
        }

        FORMATIONS[state.formation].forEach(([x, y, label], i) => {
            drawPlayer(state.slots[i], fx + x * fw / 100, fy + y * fh / 100 - 18, 58, label);
        });

        // Bank (nur belegte Plaetze)
        const subs = state.slots.slice(STARTERS).filter(Boolean);
        let footerY = H - 60;
        if (subs.length) {
            ctx.fillStyle = GOLD;
            ctx.font = `600 34px ${BODY}`;
            ctx.textAlign = "center";
            ctx.fillText("BENCH", W / 2, fy + fh + 58);
            const r = 34, step = Math.min(112, (W - 80) / subs.length);
            const startX = W / 2 - (subs.length - 1) * step / 2;
            subs.forEach((id, i) => drawPlayer(id, startX + i * step, fy + fh + 120, r, ""));
        }

        // Fuss
        ctx.textAlign = "center";
        ctx.fillStyle = "#ffffff";
        ctx.font = `700 40px ${BODY}`;
        ctx.fillText(username ? `@${username}` : "Visca Barça", W / 2, footerY);

        return new Promise((resolve, reject) =>
            canvas.toBlob(blob => blob ? resolve(blob) : reject(), "image/png"));
    }

    render();

    // Nach dem Speichern wurde die Seite neu geladen -> kurz bestaetigen
    try {
        if (sessionStorage.getItem(SAVED_FLAG)) {
            sessionStorage.removeItem(SAVED_FLAG);
            setStatus("Lineup saved ✓", "ok");
        }
    } catch { }
})();
