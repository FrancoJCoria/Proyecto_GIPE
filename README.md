# GIPE

Sistema web MVC (ASP.NET Core, .NET 10, MySQL) para un emprendimiento pequeño: permite cargar insumos, armar productos con su receta, calcular costos y precio sugerido, registrar ventas y ver las ganancias.

**Problema que resuelve:** el dueño no sabe cuánto le cuesta realmente cada producto ni cuánto gana. GIPE calcula el costo a partir de los insumos, descuenta stock al vender y muestra ingresos, gastos y ganancia neta.

## Como levantarlo

Requisitos: .NET 10 SDK y MySQL 8+.

1. Crear la base: `mysql -u root -p < data/script.sql`
2. Copiar `.env.example` a `.env` y completar `DB_PASSWORD` y `JWT_SECRET` (mínimo 32 caracteres).
3. Ejecutar: `dotnet run`

Los usuarios de prueba se crean solos al iniciar en modo Development (o con `SEED_USUARIOS=true` en `.env`).

## Usuarios de prueba

| Rol | Email | Contraseña |
|---|---|---|
| Dueño | dueno@gipe.com | Admin123! |
| Empleado | empleado@gipe.com | Empleado123! |

## Modelo de datos

8 tablas: `usuario`, `insumo`, `producto`, `producto_insumo`, `cliente`, `venta`, `venta_item`, `gasto`.

- **1:N:** `venta` → `venta_item`, `usuario` → `venta`, `cliente` → `venta`.
- **N:M:** `producto` ↔ `insumo` mediante `producto_insumo` (la receta).

Esquema completo en [data/script.sql](data/script.sql). DER: _pendiente, agregar en `/docs/der.png`_.

## Matriz de evidencias

| Requerimiento mínimo | Estado | Código | Evidencia |
|---|---|---|---|
| 1. 6+ tablas, 1:N y N:M | ✅ Cumplido | [script.sql](data/script.sql), [Models/Entities](Models/Entities) | DER pendiente |
| 2. Login, roles y avatar | ✅ Cumplido | [AccountController.cs](Controllers/AccountController.cs), [UsuariosController.cs](Controllers/UsuariosController.cs) | Captura pendiente |
| 3. Manejo de archivos (extra) | ✅ Cumplido | [ProductosController.cs](Controllers/ProductosController.cs) (portada del producto), [ServicioImagenes.cs](Models/Servicios/ServicioImagenes.cs) | Captura pendiente |
| 4. ABM en Vue.js + AJAX | ✅ Cumplido | [productos-vue.js](wwwroot/js/productos-vue.js), [Spa.cshtml](Views/Productos/Spa.cshtml) (menú "Productos (Vue)") | GIF pendiente |
| 5. Paginado por servidor | ✅ Cumplido | [AyudantePaginacion.cs](Models/Comunes/AyudantePaginacion.cs), [ResultadoPaginado.cs](Models/Comunes/ResultadoPaginado.cs) | Captura de red con `pagina=2` pendiente |
| 6. Selección asíncrona (AJAX) | ✅ Cumplido | [ventas.js](wwwroot/js/ventas.js), [receta.js](wwwroot/js/receta.js) (Select2 contra la API) | GIF pendiente |
| 7. Web API + JWT | ✅ Cumplido | [Controllers/Api](Controllers/Api), [colección Postman](data/GIPE_API.postman_collection.json) | Captura de Postman pendiente |

Funcionalidad restringida por rol: ABM de usuarios, insumos, gastos y ganancias solo para el Dueño; el Empleado solo lee productos.


