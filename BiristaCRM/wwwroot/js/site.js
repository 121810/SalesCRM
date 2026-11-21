// SalesFlow CRM — Shared UI behaviors
document.addEventListener("DOMContentLoaded", function () {
    initToolbarEnterSearch();
    initPasswordToggles();
    initRememberMeEmail();
});

/**
 * Pressing Enter in any toolbar / filter form submits the search.
 */
function initToolbarEnterSearch() {
    var forms = document.querySelectorAll("form.sf-toolbar, form[data-sf-search]");
    forms.forEach(function (form) {
        form.addEventListener("keydown", function (e) {
            if (e.key !== "Enter") return;

            var target = e.target;
            if (!target) return;

            // Allow Enter in textareas; submit from inputs and selects
            if (target.tagName === "TEXTAREA") return;
            if (target.tagName !== "INPUT" && target.tagName !== "SELECT") return;
            if (target.type === "submit" || target.type === "button" || target.type === "reset") return;

            e.preventDefault();
            if (typeof form.requestSubmit === "function") {
                form.requestSubmit();
            } else {
                form.submit();
            }
        });
    });

    // Named search fields outside toolbars (safety net)
    document.querySelectorAll('input[name="Search"], input[name="search"]').forEach(function (input) {
        if (input.closest("form.sf-toolbar") || input.closest("form[data-sf-search]")) return;

        input.addEventListener("keydown", function (e) {
            if (e.key !== "Enter") return;
            var form = input.closest("form");
            if (!form) return;
            e.preventDefault();
            if (typeof form.requestSubmit === "function") {
                form.requestSubmit();
            } else {
                form.submit();
            }
        });
    });
}

/**
 * Add show/hide eye toggle to every password input.
 */
function initPasswordToggles() {
    document.querySelectorAll('input[type="password"]').forEach(function (input) {
        if (input.dataset.sfPasswordReady === "1") return;
        input.dataset.sfPasswordReady = "1";

        var wrap = input.closest(".sf-password-wrap, .sf-login-input");
        if (!wrap) {
            wrap = document.createElement("div");
            wrap.className = "sf-password-wrap";
            input.parentNode.insertBefore(wrap, input);
            wrap.appendChild(input);
        } else {
            wrap.classList.add("sf-password-wrap");
        }

        if (wrap.querySelector(".sf-password-toggle")) return;

        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = "sf-password-toggle";
        btn.setAttribute("aria-label", "Show password");
        btn.setAttribute("tabindex", "0");
        btn.innerHTML = '<i class="bi bi-eye" aria-hidden="true"></i>';

        btn.addEventListener("click", function () {
            var showing = input.type === "text";
            input.type = showing ? "password" : "text";
            btn.setAttribute("aria-label", showing ? "Show password" : "Hide password");
            btn.innerHTML = showing
                ? '<i class="bi bi-eye" aria-hidden="true"></i>'
                : '<i class="bi bi-eye-slash" aria-hidden="true"></i>';
            input.focus();
        });

        wrap.appendChild(btn);
    });
}

/**
 * Prefill / persist login email when Remember Me is used (client assist).
 * Session longevity is handled by the auth cookie on the server.
 */
function initRememberMeEmail() {
    var form = document.querySelector("form.sf-login-form");
    if (!form) return;

    var emailInput = form.querySelector('input[name="Email"], input[type="email"]');
    var rememberInput = form.querySelector('input[name="RememberMe"]');
    if (!emailInput || !rememberInput) return;

    var storageKey = "sf_remember_email";

    try {
        var saved = localStorage.getItem(storageKey);
        if (saved && !emailInput.value) {
            emailInput.value = saved;
            rememberInput.checked = true;
        }
    } catch (err) {
        /* ignore storage errors */
    }

    form.addEventListener("submit", function () {
        try {
            if (rememberInput.checked && emailInput.value.trim()) {
                localStorage.setItem(storageKey, emailInput.value.trim());
            } else {
                localStorage.removeItem(storageKey);
            }
        } catch (err) {
            /* ignore */
        }
    });
}
