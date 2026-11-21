// SalesFlow CRM — Layout shell
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sfSidebar");
    const overlay = document.getElementById("sfSidebarOverlay");
    const menuBtn = document.getElementById("sfMenuBtn");

    if (!sidebar || !menuBtn) return;

    function isMobile() {
        return window.matchMedia("(max-width: 992px)").matches;
    }

    function closeMobile() {
        sidebar.classList.remove("open");
        if (overlay) overlay.classList.remove("show");
    }

    function openMobile() {
        sidebar.classList.add("open");
        if (overlay) overlay.classList.add("show");
    }

    menuBtn.addEventListener("click", function () {
        if (isMobile()) {
            if (sidebar.classList.contains("open")) {
                closeMobile();
            } else {
                openMobile();
            }
        } else {
            sidebar.classList.toggle("collapsed");
        }
    });

    if (overlay) {
        overlay.addEventListener("click", closeMobile);
    }

    window.addEventListener("resize", function () {
        if (!isMobile()) {
            closeMobile();
        } else {
            sidebar.classList.remove("collapsed");
        }
    });
});
