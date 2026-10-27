(function () {
    "use strict";

    var canvas = document.getElementById("grafico-ganancias");
    if (!canvas || !window.Chart) {
        return;
    }

    var nombres = JSON.parse(canvas.dataset.nombres);
    var ganancias = JSON.parse(canvas.dataset.ganancias);

    function dinero(valor) {
        return "$ " + valor.toLocaleString("es-AR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    new Chart(canvas, {
        type: "bar",
        data: {
            labels: nombres,
            datasets: [{
                label: "Ganancia",
                data: ganancias,
                backgroundColor: "#0d6efd",
                borderRadius: 4,
                maxBarThickness: 24
            }]
        },
        options: {
            indexAxis: "y",
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (contexto) {
                            return "Ganancia: " + dinero(contexto.parsed.x);
                        }
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        callback: function (valor) {
                            return "$ " + valor.toLocaleString("es-AR");
                        }
                    }
                },
                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
})();
