// Fase 3 - Alta de ventas: filas de items dinamicas con Select2 (precio y stock visibles,
// subtotal y total calculados, aviso de stock insuficiente).
(function () {
    "use strict";

    var $root = $("#items-venta");
    if ($root.length === 0 || !window.jQuery) {
        return;
    }

    var $contenedor = $("#items-filas");

    // Plantilla pristina: se clona ANTES de inicializar Select2 para que la copia
    // no arrastre el markup generado por Select2 ni ids duplicados.
    var $plantilla = $contenedor.find(".fila-item").first().clone();

    function opcionesSelect2() {
        return {
            width: "100%",
            placeholder: "Elegir producto...",
            minimumResultsForSearch: 4
        };
    }

    function iniciarSelect2($contexto) {
        $contexto.find(".js-producto").each(function () {
            var $select = $(this);
            if ($select.data("select2")) {
                return;
            }
            // Solo si Select2 se cargo; sin el plugin el <select> nativo sigue funcionando.
            if ($.fn && $.fn.select2) {
                $select.select2(opcionesSelect2());
            }
        });

        // Cliente con busqueda AJAX: consulta /Clientes/Buscar a medida que se escribe.
        $contexto.find(".js-cliente").each(function () {
            var $select = $(this);
            if ($select.data("select2")) {
                return;
            }
            if ($.fn && $.fn.select2) {
                $select.select2({
                    placeholder: "Buscar cliente ( AJAX )...",
                    allowClear: true,
                    minimumInputLength: 2,
                    width: "100%",
                    ajax: {
                        url: "/Clientes/Buscar",
                        dataType: "json",
                        delay: 250,
                        data: function (params) {
                            return { term: params.term };
                        },
                        processResults: function (data) {
                            return data; // formato select2: { results: [ {id, text} ] }
                        }
                    }
                });
            }
        });
    }

    function leerNumero($elemento) {
        var valor = parseFloat($elemento.val());
        return isNaN(valor) ? 0 : valor;
    }

    function formatoMoneda(valor) {
        return "$ " + valor.toLocaleString("es-AR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function actualizarFila($fila) {
        var $select = $fila.find(".js-producto");
        var $cantidad = $fila.find(".js-cantidad");
        var $opcion = $select.find("option:selected");

        var precio = parseFloat($opcion.data("precio"));
        var stock = parseFloat($opcion.data("stock"));
        precio = isNaN(precio) ? 0 : precio;
        var stockConocido = !isNaN(stock);

        var cantidad = leerNumero($cantidad);
        var subtotal = precio * cantidad;

        $fila.find(".js-subtotal").text(formatoMoneda(subtotal));

        // Aviso visual de stock insuficiente (el servidor tambien valida y muestra el error).
        if (stockConocido && cantidad > 0 && cantidad > stock) {
            $cantidad.addClass("is-invalid");
            $fila.find(".js-stock")
                .removeClass("text-muted")
                .addClass("text-danger")
                .text("Stock disponible: " + stock + " (insuficiente)");
        } else {
            $cantidad.removeClass("is-invalid");
            $fila.find(".js-stock")
                .removeClass("text-danger")
                .addClass($opcion.length ? "text-muted" : "")
                .text(stockConocido ? "Stock disponible: " + stock : "");
        }

        actualizarTotal();
    }

    function actualizarTotal() {
        var total = 0;

        $contenedor.find(".fila-item").each(function () {
            var $fila = $(this);
            var $opcion = $fila.find(".js-producto option:selected");
            var precio = parseFloat($opcion.data("precio"));
            var cantidad = parseFloat($fila.find(".js-cantidad").val());
            precio = isNaN(precio) ? 0 : precio;
            cantidad = isNaN(cantidad) ? 0 : cantidad;
            total += precio * cantidad;
        });

        $("#total-venta").text(formatoMoneda(total));
    }

    function renumerar() {
        $contenedor.find(".fila-item").each(function (indice) {
            $(this).find("[data-vinculo]").each(function () {
                this.name = "Items[" + indice + "]." + $(this).data("vinculo");
            });
        });
    }

    function agregarFila() {
        var $nueva = $plantilla.clone();

        if ($nueva.prop("tagName") === "TEMPLATE") {
            $nueva = $($nueva.html());
        }

        $nueva.find(".js-producto").removeClass("is-invalid").val("");
        $nueva.find(".js-cantidad").removeClass("is-invalid").val("1");
        $nueva.find(".js-subtotal").text(formatoMoneda(0));
        $nueva.find(".js-stock").text("").removeClass("text-danger");

        $contenedor.append($nueva);

        renumerar();
        iniciarSelect2($nueva);
        actualizarTotal();
    }

    //------ eventos delegados en el contenedor para cubrir filas agregadas ------
    $root.on("change", ".js-producto", function () {
        actualizarFila($(this).closest(".fila-item"));
    });

    $root.on("input", ".js-cantidad", function () {
        actualizarFila($(this).closest(".fila-item"));
    });

    $root.on("click", ".js-quitar-fila", function () {
        $(this).closest(".fila-item").remove();

        // Siempre queda al menos una fila para que el usuario cargue un item.
        if ($contenedor.find(".fila-item").length === 0) {
            agregarFila();
            return;
        }

        renumerar();
        actualizarTotal();
    });

    $("#boton-agregar-item").on("click", function () {
        agregarFila();
    });

    //------ arranque ------
    // El select de cliente esta fuera de los items: se pasa $(document) para cubrirlo.
    iniciarSelect2($(document));
    renumerar();
    $contenedor.find(".fila-item").each(function () {
        actualizarFila($(this));
    });
} ());
