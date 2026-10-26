// Fase 4 - Calculadora en vivo del formulario de productos: combina el costo de insumos
// de la receta (dato del servidor) con costo extra, margen objetivo y precio que carga
// el usuario. Muestra costo total, precio sugerido, margen real y ganancia por unidad,
// y permite aplicar el precio sugerido al campo de venta sin recargar la pagina.
(function () {
    "use strict";

    var $panel = $("#calculadora-producto");
    if ($panel.length === 0 || !window.jQuery) {
        return;
    }

    var $inputCostoInsumos = $("#CostoInsumos");
    var $inputCostoExtra = $("#CostoExtra");
    var $inputMargen = $("#MargenObjetivo");
    var $inputPrecio = $("#PrecioActual");

    function leerNumero($elemento) {
        var valor = parseFloat(String($elemento.val()).replace(",", "."));
        return isNaN(valor) ? 0 : valor;
    }

    function redondear2(valor) {
        return Math.round((valor + Number.EPSILON) * 100) / 100;
    }

    function dinero(valor) {
        return "$ " + valor.toLocaleString("es-AR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function porcentaje(valor) {
        return valor.toLocaleString("es-AR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }) + " %";
    }

    function calcular() {
        var costoInsumos = leerNumero($inputCostoInsumos);
        var costoExtra = leerNumero($inputCostoExtra);
        var margen = leerNumero($inputMargen);

        // Mismo recorte que hace el servidor: margen valido entre 0 y 100.
        if (margen < 0) { margen = 0; }
        if (margen > 100) { margen = 100; }

        var precio = leerNumero($inputPrecio);
        var costoTotal = redondear2(costoInsumos + costoExtra);
        var sugerido = redondear2(costoTotal * (1 + margen / 100));
        var ganancia = redondear2(precio - costoTotal);
        var margenReal = costoTotal > 0 ? redondear2((precio - costoTotal) / costoTotal * 100) : 0;

        $("#mostrar-costo-insumos").text(dinero(costoInsumos));
        $("#mostrar-costo-total").text(dinero(costoTotal));
        $("#mostrar-precio-sugerido").text(dinero(sugerido));

        $("#mostrar-margen-actual")
            .text(porcentaje(margenReal))
            .toggleClass("text-danger fw-bold", margenReal < 0);

        $("#mostrar-ganancia")
            .text(dinero(ganancia))
            .toggleClass("text-danger fw-bold", ganancia < 0);

        // En negativo, el costo supera el precio de venta.
        $panel.toggleClass("border-danger", ganancia < 0);

        // Guardado para el boton "Usar precio sugerido".
        $panel.data("precioSugerido", sugerido);
    }

    $("#btn-usar-sugerido").on("click", function () {
        var sugerido = $panel.data("precioSugerido");

        if (typeof sugerido === "number") {
            $inputPrecio.val(sugerido.toFixed(2));
            calcular();
        }
    });

    $inputCostoExtra.on("input", calcular);
    $inputMargen.on("input", calcular);
    $inputPrecio.on("input", calcular);

    calcular();
} ());
