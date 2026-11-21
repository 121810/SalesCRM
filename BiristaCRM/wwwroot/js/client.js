// SalesFlow CRM — Client module
document.addEventListener("DOMContentLoaded", function () {
    initializeTooltips();
    initializeCopyEmail();
    initializeDeleteConfirmation();
    initializeSearch();
});

function initializeTooltips() {
    document.querySelectorAll("[title]").forEach(function (element) {
        new bootstrap.Tooltip(element);
    });
}

function initializeCopyEmail() {
    document.querySelectorAll(".btn-copy").forEach(function (button) {
        button.addEventListener("click", function () {
            const email = this.dataset.email;
            navigator.clipboard.writeText(email);
            showToast("Email copied successfully.");
        });
    });
}

function showToast(message) {
    const toastElement = document.getElementById("copyToast");
    if (!toastElement) return;

    toastElement.querySelector(".toast-body").innerHTML =
        '<i class="bi bi-check-circle-fill me-2"></i>' + message;

    new bootstrap.Toast(toastElement).show();
}

function initializeDeleteConfirmation() {
    document.querySelectorAll(".btn-delete").forEach(function (button) {
        button.addEventListener("click", function () {
            const modalEl = document.getElementById("deleteModal");
            if (!modalEl) return;
            new bootstrap.Modal(modalEl).show();
        });
    });
}

function initializeSearch() {
    // Enter-to-search is handled globally in site.js for all toolbar forms.
}
