const appointmentsById = new Map();

document.addEventListener("DOMContentLoaded", async () => {
    if (!requireLogin()) return;

    const form = document.getElementById("appointmentForm");
    const startInput = document.getElementById("startAt");
    startInput.min = minimumDateTimeValue(30);

    form.addEventListener("submit", saveAppointment);
    document.getElementById("cancelAppointmentEdit").addEventListener("click", resetAppointmentForm);

    try {
        await loadFormData();
        await loadAppointments();
    } catch (error) {
        showMessage(error.message, "error");
    }
});

async function loadFormData() {
    const carSelect = document.getElementById("carId");
    const serviceSelect = document.getElementById("serviceItemId");
    carSelect.replaceChildren();
    serviceSelect.replaceChildren();

    const [cars, services] = await Promise.all([
        api("/api/cars"),
        api("/api/services")
    ]);

    carSelect.appendChild(createOption("", "Válassz autót"));
    for (const car of cars) {
        carSelect.appendChild(createOption(car.id, `${car.plateNumber} – ${car.brand} ${car.model}`));
    }

    serviceSelect.appendChild(createOption("", "Válassz szolgáltatást"));
    for (const service of services) {
        serviceSelect.appendChild(createOption(
            service.id,
            `${service.name} – ${formatMoney(service.price)} (${service.estimatedMinutes} perc)`
        ));
    }

    const submitButton = document.getElementById("saveAppointmentButton");
    submitButton.disabled = cars.length === 0 || services.length === 0;
    document.getElementById("noCarHelp").classList.toggle("hidden", cars.length > 0);
}

async function saveAppointment(event) {
    event.preventDefault();
    hideMessage();

    const form = event.currentTarget;
    const data = Object.fromEntries(new FormData(form));
    const appointmentId = Number(data.appointmentId || 0);
    delete data.appointmentId;

    const payload = appointmentId
        ? { startAt: data.startAt, note: data.note }
        : {
            carId: Number(data.carId),
            serviceItemId: Number(data.serviceItemId),
            startAt: data.startAt,
            note: data.note
        };

    try {
        await api(appointmentId ? `/api/appointments/${appointmentId}` : "/api/appointments", {
            method: appointmentId ? "PUT" : "POST",
            body: JSON.stringify(payload)
        });

        showMessage(appointmentId ? "Az időpont módosítva lett." : "Az időpont rögzítve lett.");
        resetAppointmentForm();
        await loadAppointments();
    } catch (error) {
        showMessage(error.message, "error");
    }
}

async function loadAppointments() {
    const table = document.getElementById("appointmentsTable");
    table.replaceChildren();
    appointmentsById.clear();

    const appointments = await api("/api/appointments");
    if (appointments.length === 0) {
        renderEmptyRow(table, 6, "Még nincs rögzített időpont.");
        return;
    }

    for (const item of appointments) {
        appointmentsById.set(item.id, item);
        const row = document.createElement("tr");
        appendTextCell(row, formatDate(item.startAt));
        appendTextCell(row, item.plateNumber);
        appendTextCell(row, `${item.serviceName} (${item.estimatedMinutes} perc)`);

        const statusCell = appendTextCell(row, "");
        const badge = document.createElement("span");
        badge.className = `badge status-${item.status.toLowerCase()}`;
        badge.textContent = statusLabel(item.status);
        statusCell.appendChild(badge);

        appendTextCell(row, item.note || "–");
        const actionCell = appendTextCell(row, "", "actions");

        if (item.status !== "Completed" && item.status !== "Cancelled") {
            actionCell.appendChild(createButton(
                "Módosítás",
                "small secondary",
                () => startAppointmentEdit(item.id)
            ));
        }

        actionCell.appendChild(createButton(
            "Törlés",
            "small danger",
            () => deleteAppointment(item.id)
        ));
        table.appendChild(row);
    }
}

function startAppointmentEdit(id) {
    const item = appointmentsById.get(id);
    if (!item) return;

    document.getElementById("appointmentId").value = item.id;
    document.getElementById("carId").value = item.carId;
    document.getElementById("serviceItemId").value = item.serviceItemId;
    document.getElementById("startAt").value = toLocalInputValue(item.startAt);
    document.getElementById("note").value = item.note || "";
    document.getElementById("carId").disabled = true;
    document.getElementById("serviceItemId").disabled = true;
    document.getElementById("appointmentFormTitle").textContent = "Időpont módosítása";
    document.getElementById("saveAppointmentButton").textContent = "Módosítás mentése";
    document.getElementById("cancelAppointmentEdit").classList.remove("hidden");
    document.getElementById("appointmentFormTitle").scrollIntoView({ behavior: "smooth", block: "start" });
}

function resetAppointmentForm() {
    const form = document.getElementById("appointmentForm");
    form.reset();
    document.getElementById("appointmentId").value = "";
    document.getElementById("carId").disabled = false;
    document.getElementById("serviceItemId").disabled = false;
    document.getElementById("startAt").min = minimumDateTimeValue(30);
    document.getElementById("appointmentFormTitle").textContent = "Új időpont foglalása";
    document.getElementById("saveAppointmentButton").textContent = "Időpont foglalása";
    document.getElementById("cancelAppointmentEdit").classList.add("hidden");
}

async function deleteAppointment(id) {
    if (!confirm("Biztosan törlöd az időpontot?")) return;

    try {
        await api(`/api/appointments/${id}`, { method: "DELETE" });
        showMessage("Az időpont törölve lett.");
        resetAppointmentForm();
        await loadAppointments();
    } catch (error) {
        showMessage(error.message, "error");
    }
}
