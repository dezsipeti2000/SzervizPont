const carsById = new Map();

document.addEventListener("DOMContentLoaded", async () => {
    if (!requireLogin()) return;

    const form = document.getElementById("carForm");
    const yearInput = document.getElementById("year");
    yearInput.max = String(new Date().getFullYear() + 1);

    form.addEventListener("submit", saveCar);
    document.getElementById("cancelCarEdit").addEventListener("click", resetCarForm);

    await loadCars();
});

async function saveCar(event) {
    event.preventDefault();
    hideMessage();

    const form = event.currentTarget;
    const data = Object.fromEntries(new FormData(form));
    const carId = Number(data.carId || 0);
    delete data.carId;
    data.year = Number(data.year);

    try {
        await api(carId ? `/api/cars/${carId}` : "/api/cars", {
            method: carId ? "PUT" : "POST",
            body: JSON.stringify(data)
        });

        showMessage(carId ? "Az autó módosítva lett." : "Az autó mentve lett.");
        resetCarForm();
        await loadCars();
    } catch (error) {
        showMessage(error.message, "error");
    }
}

async function loadCars() {
    const table = document.getElementById("carsTable");
    table.replaceChildren();
    carsById.clear();

    try {
        const cars = await api("/api/cars");
        if (cars.length === 0) {
            renderEmptyRow(table, 5, "Még nincs rögzített autó.");
            return;
        }

        for (const car of cars) {
            carsById.set(car.id, car);
            const row = document.createElement("tr");
            appendTextCell(row, car.plateNumber);
            appendTextCell(row, car.brand);
            appendTextCell(row, car.model);
            appendTextCell(row, car.year);

            const actionCell = appendTextCell(row, "", "actions");
            actionCell.append(
                createButton("Módosítás", "small secondary", () => startCarEdit(car.id)),
                createButton("Törlés", "small danger", () => deleteCar(car.id))
            );
            table.appendChild(row);
        }
    } catch (error) {
        showMessage(error.message, "error");
    }
}

function startCarEdit(id) {
    const car = carsById.get(id);
    if (!car) return;

    document.getElementById("carId").value = car.id;
    document.getElementById("plateNumber").value = car.plateNumber;
    document.getElementById("brand").value = car.brand;
    document.getElementById("model").value = car.model;
    document.getElementById("year").value = car.year;
    document.getElementById("carFormTitle").textContent = "Autó módosítása";
    document.getElementById("saveCarButton").textContent = "Módosítás mentése";
    document.getElementById("cancelCarEdit").classList.remove("hidden");
    document.getElementById("carFormTitle").scrollIntoView({ behavior: "smooth", block: "start" });
}

function resetCarForm() {
    const form = document.getElementById("carForm");
    form.reset();
    document.getElementById("carId").value = "";
    document.getElementById("carFormTitle").textContent = "Új autó rögzítése";
    document.getElementById("saveCarButton").textContent = "Autó mentése";
    document.getElementById("cancelCarEdit").classList.add("hidden");
}

async function deleteCar(id) {
    if (!confirm("Biztosan törlöd az autót?")) return;

    try {
        await api(`/api/cars/${id}`, { method: "DELETE" });
        showMessage("Az autó törölve lett.");
        resetCarForm();
        await loadCars();
    } catch (error) {
        showMessage(error.message, "error");
    }
}
