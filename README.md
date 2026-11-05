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

Esquema completo en [data/script.sql](data/script.sql). DER: [docs/der.png](docs/der.png).

## Matriz de evidencias

Las evidencias se guardan en [docs/evidencias/](docs/evidencias).

| Requerimiento mínimo | Estado | Código | Evidencia |
|---|---|---|---|
| 1. 8 tablas, 1:N y N:M | ✅ Cumplido | [script.sql](data/script.sql) (tablas y FKs), [Models/Entities](Models/Entities) | [docs/der.png](docs/der.png) (DER) |
| 2. Login, roles y avatar | ✅ Cumplido | [AccountController.cs#L46](Controllers/AccountController.cs#L46) (Login), [AccountController.cs#L178](Controllers/AccountController.cs#L178) (avatar), [Program.cs#L94](Program.cs#L94) (política rol Dueño) | `docs/evidencias/perfil-avatar.png` (captura del perfil con avatar) |
| 3. Manejo de archivos (extra) | ✅ Cumplido | [ProductosController.cs#L62](Controllers/ProductosController.cs#L62) (alta con `IFormFile`), [ServicioImagenes.cs#L32](Models/Servicios/ServicioImagenes.cs#L32) (guardado) | `docs/evidencias/subida-producto.gif` (subida de imagen del producto) |
| 4. ABM en Vue.js + AJAX | ✅ Cumplido | [productos-vue.js](wwwroot/js/productos-vue.js), [Spa.cshtml](Views/Productos/Spa.cshtml) (menú "Productos (Vue)") | `docs/evidencias/abm-vue.gif` (ABM dinámico) |
| 5. Paginado por servidor | ✅ Cumplido | [AyudantePaginacion.cs](Models/Comunes/AyudantePaginacion.cs), [ResultadoPaginado.cs](Models/Comunes/ResultadoPaginado.cs), [Api/ProductosController.cs#L20](Controllers/Api/ProductosController.cs#L20) (`pagina` + `tamano`) | `docs/evidencias/red-pagina2.png` (inspector de red con `pagina=2`) |
| 6. Selección asíncrona (AJAX) | ✅ Cumplido | [ventas.js#L22](wwwroot/js/ventas.js#L22) (productos) y [ventas.js#L63](wwwroot/js/ventas.js#L63) (clientes), [receta.js#L13](wwwroot/js/receta.js#L13) (insumos), endpoints [Api/ProductosController.cs#L20](Controllers/Api/ProductosController.cs#L20), [Api/ClientesController.cs#L20](Controllers/Api/ClientesController.cs#L20), [Api/InsumosController.cs#L20](Controllers/Api/InsumosController.cs#L20) | `docs/evidencias/select-busqueda.gif` (desplegable buscando) |
| 7. Web API + JWT | ✅ Cumplido | [Api/AuthController.cs#L31](Controllers/Api/AuthController.cs#L31) (login → token), [Api/AuthController.cs#L76](Controllers/Api/AuthController.cs#L76) (perfil con Bearer), [Program.cs#L75](Program.cs#L75) (config JWT), [colección Postman](data/GIPE_API.postman_collection.json) | `docs/evidencias/postman-jwt.png` (Postman con Bearer Token) |

Funcionalidad restringida por rol: ABM de usuarios, insumos, gastos y ganancias solo para el Dueño; el Empleado solo lee productos.


