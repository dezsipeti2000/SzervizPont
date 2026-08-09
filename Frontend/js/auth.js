document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    if (loginForm) {
        loginForm.addEventListener("submit", async event => {
            event.preventDefault();
            hideMessage();

            const formData = Object.fromEntries(new FormData(loginForm));

            try {
                const result = await api("/api/auth/login", {
                    method: "POST",
                    body: JSON.stringify(formData)
                });

                saveUser(result.user);
                window.location.href = "dashboard.html";
            } catch (error) {
                showMessage(error.message, "error");
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener("submit", async event => {
            event.preventDefault();
            hideMessage();

            const formData = Object.fromEntries(new FormData(registerForm));

            try {
                const result = await api("/api/auth/register", {
                    method: "POST",
                    body: JSON.stringify(formData)
                });

                saveUser(result.user);
                window.location.href = "dashboard.html";
            } catch (error) {
                showMessage(error.message, "error");
            }
        });
    }
});
