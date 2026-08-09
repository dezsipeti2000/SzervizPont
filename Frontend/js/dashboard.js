document.addEventListener("DOMContentLoaded", async () => {
    if (!requireLogin()) return;

    const user = getUser();
    document.getElementById("welcomeText").textContent = `Bejelentkezve: ${user.name} (${user.email})`;

    try {
        const adminQuery = user.role === "Admin" ? "?all=true" : "";
        const [cars, appointments, services] = await Promise.all([
            api(`/api/cars${adminQuery}`),
            api(`/api/appointments${adminQuery}`),
            api("/api/services")
        ]);

        document.getElementById("carCount").textContent = cars.length;
        document.getElementById("appointmentCount").textContent = appointments.length;
        document.getElementById("serviceCount").textContent = services.length;

        if (user.role === "Admin") {
            document.getElementById("carLabel").textContent = "Összes rögzített autó";
            document.getElementById("appointmentLabel").textContent = "Összes időpont";
        }
    } catch (error) {
        showMessage(error.message, "error");
    }
});
