// Busqueda asincrona de insumos para la receta del producto (vista Detalles).
// El select se llena con Select2 + AJAX contra /api/insumos: el servidor filtra por
// nombre y trae solo la pagina pedida (maximo 25 resultados). Nunca se precargan
// todos los insumos de la base en las opciones.
(function () {
    "use strict";

    var $select = $("#insumoId");
    if ($select.length === 0 || !window.jQuery || !$.fn.select2) {
        return;
    }

    $select.select2({
        width: "100%",
        placeholder: "Buscar insumo...",
        minimumInputLength: 0,
        allowClear: true,
        ajax: {
            url: "/api/insumos",
            dataType: "json",
            delay: 250,
            data: function (params) {
                return {
                    pagina: params.page || 1,
                    busqueda: params.term || "",
                    tamano: 25
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                var insumos = (data.insumos || []).map(function (insumo) {
                    return {
                        id: insumo.id,
                        text: insumo.texto
                    };
                });

                return {
                    results: insumos,
                    pagination: { more: params.page * data.tamano < data.total }
                };
            },
            cache: true
        }
    });
} ());
