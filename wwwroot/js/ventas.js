// Fase 5 - Alta de ventas: filas de items dinamicas con Select2 en modo AJAX.
// Producto y cliente NO se precargan en el <select>: cada desplegable busca contra
// /api/productos y /api/clientes (paginado en el servidor), con precio y stock
// visibles, subtotal y total calculados, y aviso de stock insuficiente.
(function () {
    "use strict";

    var $root = $("#items-venta");
    if ($root.length === 0 || !window.jQuery) {
        return;
    }

    var TAMANO_BUSQUEDA = 20;

    var $contenedor = $("#items-filas");

    // Plantilla pristina: se clona ANTES de inicializar Select2 para que la copia
    // no arrastre el markup generado por Select2 ni ids duplicados.
    var $plantilla = $contenedor.find(".fila-item").first().clone();

    //------------------------------------------------------------BUSQUEDA ASINCRONA --------------------------------------------------------
    function opcionesProducto() {
        return {
            width: "100%",
            placeholder: "Elegir producto...",
            allowClear: true,
            ajax: {
                url: "/api/productos",
                dataType: "json",
                delay: 250,
                data: function (params) {
                    return {
                        pagina: params.page || 1,
                        busqueda: params.term || "",
                        tamano: TAMANO_BUSQUEDA,
                        soloActivos: true
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    var productos = (data.productos || []).map(function (producto) {
                        return {
                            id: producto.id,
                            text: producto.nombre
                                + " - " + formatoMoneda(producto.precioActual)
                                + " - stock: " + formatoCantidad(producto.stockActual),
                            precio: producto.precioActual,
                            stock: producto.stockActual
                        };
                    });

                    return {
                        results: productos,
                        pagination: { more: params.page * data.tamano < data.total }
                    };
                },
                cache: true
            }
        };
    }

    function opcionesCliente() {
        return {
            width: "100%",
            placeholder: "Sin cliente (venta mostrador)",
            allowClear: true,
            ajax: {
                url: "/api/clientes",
                dataType: "json",
                delay: 250,
                data: function (params) {
                    return {
                        pagina: params.page || 1,
                        busqueda: params.term || "",
                        tamano: TAMANO_BUSQUEDA
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    var clientes = (data.clientes || []).map(function (cliente) {
                        return {
                            id: cliente.id,
                            text: cliente.texto
                        };
                    });

                    return {
                        results: clientes,
                        pagination: { more: params.page * data.tamano < data.total }
                    };
                },
                cache: true
            }
        };
    }

    function iniciarSelect2Productos($contexto) {
        $contexto.find(".js-producto").each(function () {
            var $select = $(this);
            if ($select.data("select2")) {
                return;
            }
            // Solo si Select2 se cargo; sin el plugin el <select> nativo sigue funcionando.
            if ($.fn && $.fn.select2) {
                $select.select2(opcionesProducto());
            }
        });
    }

    function iniciarSelect2Clientes() {
        var $cliente = $("#ClienteId");
        if ($cliente.length === 0 || $cliente.data("select2")) {
            return;
        }
        if ($.fn && $.fn.select2) {
            $cliente.select2(opcionesCliente());
        }
    }

    function iniciarSelect2($contexto) {
        iniciarSelect2Productos($contexto);
        iniciarSelect2Clientes();
    }

    //------------------------------------------------------------AYUDAS --------------------------------------------------------
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

    function formatoCantidad(valor) {
        valor = isNaN(valor) ? 0 : valor;
        return valor.toLocaleString("es-AR", {
            maximumFractionDigits: 2
        });
    }

    //------------------------------------------------------------CALCULO DE FILA --------------------------------------------------------
    function datosProductoElegido($fila) {
        var $select = $fila.find(".js-producto");
        var $opcion = $select.find("option:selected");

        var datos = {
            precio: parseFloat($opcion.data("precio")),
            stock: parseFloat($opcion.data("stock"))
        };

        // Con Select2 en modo AJAX el <option> elegido se crea desde el resultado JSON;
        // el precio y el stock estan mas seguros en el objeto de datos de la seleccion.
        if (isNaN(datos.precio) && $select.data("select2")) {
            var seleccion = $select.select2("data");

            if (seleccion && seleccion.length > 0 && seleccion[0]) {
                datos.precio = parseFloat(seleccion[0].precio);
                datos.stock = parseFloat(seleccion[0].stock);
            }
        }

        return datos;
    }

    function actualizarFila($fila) {
        var $select = $fila.find(".js-producto");
        var $cantidad = $fila.find(".js-cantidad");
        var datos = datosProductoElegido($fila);

        var precio = isNaN(datos.precio) ? 0 : datos.precio;
        var stock = datos.stock;
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
                .text("Stock disponible: " + formatoCantidad(stock) + " (insuficiente)");
        } else {
            $cantidad.removeClass("is-invalid");
            $fila.find(".js-stock")
                .removeClass("text-danger")
                .addClass($select.val() ? "text-muted" : "")
                .text(stockConocido ? "Stock disponible: " + formatoCantidad(stock) : "");
        }

        actualizarTotal();
    }

    function actualizarTotal() {
        var total = 0;

        $contenedor.find(".fila-item").each(function () {
            var $fila = $(this);
            var datos = datosProductoElegido($fila);
            var precio = isNaN(datos.precio) ? 0 : datos.precio;
            var cantidad = parseFloat($fila.find(".js-cantidad").val());
            cantidad = isNaN(cantidad) ? 0 : cantidad;
            total += precio * cantidad;
        });

        $("#total-venta").text(formatoMoneda(total));
    }

    //------------------------------------------------------------FILAS DINAMICAS --------------------------------------------------------
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
    iniciarSelect2($contenedor);
    renumerar();
    $contenedor.find(".fila-item").each(function () {
        actualizarFila($(this));
    });
} ());
