// SalesFlow CRM — Dashboard
document.addEventListener("DOMContentLoaded", function () {
    startCounter();
    loadSalesChart();
});

function startCounter() {
    const counters = document.querySelectorAll(".sf-stat-value[data-count]");

    counters.forEach(function (counter) {
        const target = parseInt(counter.getAttribute("data-count"), 10);
        if (isNaN(target)) return;

        let current = 0;
        const speed = Math.max(1, Math.ceil(target / 40));
        counter.textContent = "0";

        const interval = setInterval(function () {
            current += speed;
            if (current >= target) {
                counter.textContent = target;
                clearInterval(interval);
            } else {
                counter.textContent = current;
            }
        }, 30);
    });
}

function loadSalesChart() {
    const canvas = document.getElementById("salesChart");
    if (!canvas || typeof Chart === "undefined") return;

    let labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"];
    let values = [0, 0, 0, 0, 0, 0, 0];

    try {
        if (canvas.dataset.labels) labels = JSON.parse(canvas.dataset.labels);
        if (canvas.dataset.values) values = JSON.parse(canvas.dataset.values);
    } catch (e) { /* keep defaults */ }

    const primary = getComputedStyle(document.documentElement).getPropertyValue("--sf-primary").trim() || "#0D9488";

    new Chart(canvas, {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                label: "Monthly Sales",
                data: values,
                borderColor: primary,
                backgroundColor: "rgba(13, 148, 136, 0.12)",
                fill: true,
                tension: 0.4,
                borderWidth: 2.5,
                pointRadius: 3,
                pointBackgroundColor: primary
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: "#94A3B8", font: { size: 12 } }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: "#F1F5F9" },
                    ticks: { color: "#94A3B8", font: { size: 12 } }
                }
            }
        }
    });
}
