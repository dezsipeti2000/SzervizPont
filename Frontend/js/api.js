const userKey = "szervizpont_user";

function getUser() {
    const savedUser = localStorage.getItem(userKey);
    if (!savedUser) return null;

    try {
        return JSON.parse(savedUser);
    } catch {
        localStorage.removeItem(userKey);
        return null;
    }
}

function saveUser(user) {
    localStorage.setItem(userKey, JSON.stringify(user));
}

function clearUser() {
    localStorage.removeItem(userKey);
}

function requireLogin() {
    if (getUser()) return true;
    window.location.href = "login.html";
    return false;
}

function requireAdmin() {
    const user = getUser();

    if (user && user.role === "Admin") return true;

    window.location.href = user ? "dashboard.html" : "login.html";
    return false;
}

async function api(path, options = {}) {
    const headers = new Headers(options.headers || {});

    if (options.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
    }

    let response;

    try {
        response = await fetch(path, {
            ...options,
            headers,
            credentials: "same-origin"
        });
    } catch {
        throw new Error("A szerver nem érhető el. Ellenőrizd, hogy fut-e az alkalmazás.");
    }

    const text = await response.text();
    let data = null;

    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = { message: text };
        }
    }

    if (!response.ok) {
        if (response.status === 401 && !path.startsWith("/api/auth/")) {
            clearUser();
            window.location.href = "login.html";
        }

        if (response.status === 403) {
            throw new Error("Ehhez a művelethez nincs jogosultságod.");
        }

        throw new Error(data?.message || "Hiba történt a művelet közben.");
    }

    return data;
}

function showMessage(text, type = "ok") {
    const messageBox = document.getElementById("message");
    if (!messageBox) return;

    messageBox.textContent = text;
    messageBox.className = `message show ${type}`;
}

function hideMessage() {
    const messageBox = document.getElementById("message");
    if (!messageBox) return;

    messageBox.textContent = "";
    messageBox.className = "message";
}

function formatDate(value) {
    if (!value) return "";
    return new Date(value).toLocaleString("hu-HU");
}

function formatMoney(value) {
    return `${Number(value).toLocaleString("hu-HU")} Ft`;
}

function toLocalInputValue(value) {
    const date = new Date(value);
    const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return localDate.toISOString().slice(0, 16);
}

function minimumDateTimeValue(minutesFromNow = 30) {
    return toLocalInputValue(new Date(Date.now() + minutesFromNow * 60000));
}

function appendTextCell(row, value, className = "") {
    const cell = document.createElement("td");
    cell.textContent = value ?? "";
    cell.className = className;
    row.appendChild(cell);
    return cell;
}

function createButton(text, className, clickFunction) {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = text;
    button.className = className;
    button.addEventListener("click", clickFunction);
    return button;
}

function createOption(value, text, selected = false) {
    const option = document.createElement("option");
    option.value = value;
    option.textContent = text;
    option.selected = selected;
    return option;
}

function statusLabel(status) {
    if (status === "Pending") return "Függőben";
    if (status === "Confirmed") return "Megerősítve";
    if (status === "Completed") return "Elvégezve";
    if (status === "Cancelled") return "Lemondva";
    return status;
}

function renderEmptyRow(tableBody, columnCount, message) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = columnCount;
    cell.className = "empty-state";
    cell.textContent = message;
    row.appendChild(cell);
    tableBody.appendChild(row);
}

function setupNav() {
    const user = getUser();

    document.querySelectorAll(".login-link").forEach(link => {
        link.classList.toggle("hidden", user !== null);
    });

    document.querySelectorAll(".user-link").forEach(link => {
        link.classList.toggle("hidden", user === null);
    });

    document.querySelectorAll(".admin-link").forEach(link => {
        link.classList.toggle("hidden", user?.role !== "Admin");
    });

    const userName = document.getElementById("navUserName");
    if (userName && user) {
        userName.textContent = user.name;
    }

    document.querySelectorAll(".logout-button").forEach(button => {
        button.addEventListener("click", logout);
    });
}

async function logout() {
    try {
        await api("/api/auth/logout", { method: "POST" });
    } catch {
        // Akkor is töröljük a böngészőben tárolt felhasználói adatot.
    }

    clearUser();
    window.location.href = "index.html";
}

document.addEventListener("DOMContentLoaded", setupNav);
