const servicesById = new Map();

document.addEventListener("DOMContentLoaded", async () => {
    if (!requireAdmin()) return;

    document.getElementById("serviceForm").addEventListener("submit", saveService);
    document.getElementById("cancelServiceEdit").addEventListener("click", resetServiceForm);

    await loadAdminData();
});

async function loadAdminData() {
    try {
        await Promise.all([loadUsers(), loadServices(), loadAdminAppointments()]);
    } catch (error) {
        showMessage(error.message, "error");
    }
}

async function saveService(event) {
    event.preventDefault();
    hideMessage();

    const form = event.currentTarget;
    const data = Object.fromEntries(new FormData(form));
    const serviceId = Number(data.serviceId || 0);
    delete data.serviceId;
    data.price = Number(data.price);
    data.estimatedMinutes = Number(data.estimatedMinutes);

    try {
        await api(serviceId ? `/api/services/${serviceId}` : "/api/services", {
            method: serviceId ? "PUT" : "POST",
            body: JSON.stringify(data)
        });

        showMessage(serviceId ? "A szolgáltatás módosítva lett." : "A szolgáltatás mentve lett.");
        resetServiceForm();
        await loadServices();
    } catch (error) {
        showMessage(error.message, "error");
    }
}

async function loadUsers() {
    const table = document.getElementById("usersTable");
    table.replaceChildren();
    const users = await api("/api/users");

    if (users.length === 0) {
        renderEmptyRow(table, 3, "Nincs megjeleníthető felhasználó.");
        return;
    }

    for (const user of users) {
        const row = document.createElement("tr");
        appendTextCell(row, user.name);
        appendTextCell(row, user.email);
        appendTextCell(row, user.role === "Admin" ? "Adminisztrátor" : "Ügyfél");
        table.appendChild(row);
    }
}

async function loadServices() {
    const table = document.getElementById("servicesTable");
    table.replaceChildren();
    servicesById.clear();
    const services = await api("/api/services");

    if (services.length === 0) {
        renderEmptyRow(table, 5, "Nincs megjeleníthető szolgáltatás.");
        return;
    }

    for (const service of services) {
        servicesById.set(service.id, service);
        const row = document.createElement("tr");
        appendTextCell(row, service.name);
        appendTextCell(row, service.description || "–");
        appendTextCell(row, formatMoney(service.price));
        appendTextCell(row, `${service.estimatedMinutes} perc`);

        const actionCell = appendTextCell(row, "", "actions");
        actionCell.append(
            createButton("Módosítás", "small secondary", () => startServiceEdit(service.id)),
            createButton("Törlés", "small danger", () => deleteService(service.id))
        );
        table.appendChild(row);
    }
}

function startServiceEdit(id) {
    const service = servicesById.get(id);
    if (!service) return;

    document.getElementById("serviceId").value = service.id;
    document.getElementById("serviceName").value = service.name;
    document.getElementById("description").value = service.description;
    document.getElementById("price").value = service.price;
    document.getElementById("estimatedMinutes").value = service.estimatedMinutes;
    document.getElementById("serviceFormTitle").textContent = "Szolgáltatás módosítása";
    document.getElementById("saveServiceButton").textContent = "Módosítás mentése";
    document.getElementById("cancelServiceEdit").classList.remove("hidden");
    document.getElementById("serviceFormTitle").scrollIntoView({ behavior: "smooth", block: "start" });
}

function resetServiceForm() {
    const form = document.getElementById("serviceForm");
    form.reset();
    document.getElementById("serviceId").value = "";
    document.getElementById("serviceFormTitle").textContent = "Új szolgáltatás";
    document.getElementById("saveServiceButton").textContent = "Szolgáltatás mentése";
    document.getElementById("cancelServiceEdit").classList.add("hidden");
}

async function deleteService(id) {
    if (!confirm("Biztosan törlöd a szolgáltatást?")) return;

    try {
        await api(`/api/services/${id}`, { method: "DELETE" });
        showMessage("A szolgáltatás törölve lett.");
        resetServiceForm();
        await loadServices();
    } catch (error) {
        showMessage(error.message, "error");
    }
}

async function loadAdminAppointments() {
    const table = document.getElementById("adminAppointmentsTable");
    table.replaceChildren();
    const appointments = await api("/api/appointments?all=true");

    if (appointments.length === 0) {
        renderEmptyRow(table, 7, "Nincs megjeleníthető időpont.");
        return;
    }

    for (const item of appointments) {
        const row = document.createElement("tr");
        appendTextCell(row, formatDate(item.startAt));
        appendTextCell(row, item.userName);
        appendTextCell(row, item.plateNumber);
        appendTextCell(row, `${item.serviceName} (${item.estimatedMinutes} perc)`);
        appendTextCell(row, item.note || "–");

        const statusCell = appendTextCell(row, "");
        const select = document.createElement("select");
        select.setAttribute("aria-label", `${item.userName} időpontjának státusza`);
        for (const status of ["Pending", "Confirmed", "Completed", "Cancelled"]) {
            select.appendChild(createOption(status, statusLabel(status), status === item.status));
        }
        select.addEventListener("change", () => changeStatus(item.id, select.value));
        statusCell.appendChild(select);

        const actionCell = appendTextCell(row, "", "actions");
        actionCell.appendChild(createButton(
            "Törlés",
            "small danger",
            () => deleteAppointmentAdmin(item.id)
        ));
        table.appendChild(row);
    }
}

async function changeStatus(id, status) {
    try {
        await api(`/api/appointments/${id}/status`, {
            method: "PUT",
            body: JSON.stringify({ status })
        });
        showMessage("A státusz módosítva lett.");
        await loadAdminAppointments();
    } catch (error) {
        showMessage(error.message, "error");
        await loadAdminAppointments();
    }
}

async function deleteAppointmentAdmin(id) {
    if (!confirm("Biztosan törlöd az időpontot?")) return;

    try {
        await api(`/api/appointments/${id}`, { method: "DELETE" });
        showMessage("Az időpont törölve lett.");
        await loadAdminAppointments();
    } catch (error) {
        showMessage(error.message, "error");
    }
}
