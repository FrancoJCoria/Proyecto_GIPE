// ABM de clientes hecho con Vue 3 que consume la Web API + JWT del proyecto
// (Controllers/Api/ClientesController.cs y Controllers/Api/AuthController.cs).
// Todo el trafico es AJAX: login (emite el Bearer), listado con busqueda,
// alta, edicion y baja logica, sin recargar la pagina.
(function () {
    "use strict";

    if (typeof Vue === "undefined") {
        console.error("Vue no esta cargado.");
        return;
    }

    var CLAVE_TOKEN = "GIPE.JWT";

    var plantilla = [
        '<div>',

        // ===================== LOGIN (emite el JWT) =====================
        '<div v-if="!token" class="row justify-content-center">',
        '  <div class="col-md-6 col-lg-5">',
        '    <div class="card shadow-sm">',
        '      <div class="card-body">',
        '        <h5 class="card-title mb-3">Sesion para la API (JWT)</h5>',
        '        <form v-on:submit.prevent="ingresar">',
        '          <div class="mb-3">',
        '            <label class="form-label">Correo</label>',
        '            <input v-model.trim="emailLogin" type="email" class="form-control" autocomplete="username" />',
        '          </div>',
        '          <div class="mb-3">',
        '            <label class="form-label">Contraseña</label>',
        '            <input v-model="passwordLogin" type="password" class="form-control" autocomplete="current-password" />',
        '          </div>',
        '          <div v-if="errorLogin" class="alert alert-danger py-2 small">{{ errorLogin }}</div>',
        '          <button type="submit" class="btn btn-primary w-100" :disabled="logueando">',
        '            {{ logueando ? "Ingresando..." : "Obtener token e ingresar" }}',
        '          </button>',
        '        </form>',
        '      </div>',
        '    </div>',
        '  </div>',
        '</div>',

        // ===================== ABM =====================
        '<div v-else>',

        '  <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">',
        '    <div class="text-muted small">',
        '      Sesion JWT activa:',
        '      <strong>{{ usuarioActual ? usuarioActual.nombre : "..." }}</strong>',
        '      <span class="badge text-bg-primary ms-1">{{ usuarioActual ? usuarioActual.rol : "" }}</span>',
        '    </div>',
        '    <div class="d-flex gap-2">',
        '      <button type="button" class="btn btn-primary" v-on:click="prepararAlta" :disabled="cargando">+ Nuevo cliente</button>',
        '      <button type="button" class="btn btn-outline-secondary" v-on:click="listar" :disabled="cargando">Refrescar</button>',
        '      <button type="button" class="btn btn-outline-danger" v-on:click="salir">Salir</button>',
        '    </div>',
        '  </div>',

        '  <div v-if="mensajeOk" class="alert alert-success alert-dismissible fade show py-2">',
        '    {{ mensajeOk }}',
        '    <button type="button" class="btn-close" v-on:click="mensajeOk = \'\'"></button>',
        '  </div>',

        // ------- formulario alta/edicion (el mismo card) -------
        '  <div class="card shadow-sm mb-3">',
        '    <div class="card-header fw-bold">{{ esEdicion ? "Editar cliente #" + form.id : "Nuevo cliente" }}</div>',
        '    <div class="card-body">',
        '      <form v-on:submit.prevent="guardar">',
        '        <div class="row g-2">',
        '          <div class="col-md-4">',
        '            <label class="form-label small">Nombre *</label>',
        '            <input v-model.trim="form.nombre" class="form-control form-control-sm" maxlength="150" />',
        '          </div>',
        '          <div class="col-md-4">',
        '            <label class="form-label small">Apellido</label>',
        '            <input v-model.trim="form.apellido" class="form-control form-control-sm" maxlength="150" />',
        '          </div>',
        '          <div class="col-md-4">',
        '            <label class="form-label small">Telefono</label>',
        '            <input v-model.trim="form.telefono" class="form-control form-control-sm" maxlength="30" />',
        '          </div>',
        '          <div class="col-md-4">',
        '            <label class="form-label small">Correo</label>',
        '            <input v-model.trim="form.email" type="email" class="form-control form-control-sm" maxlength="150" />',
        '          </div>',
        '          <div class="col-md-8">',
        '            <label class="form-label small">Direccion</label>',
        '            <input v-model.trim="form.direccion" class="form-control form-control-sm" maxlength="200" />',
        '          </div>',
        '        </div>',
        '        <div v-if="errorForm" class="alert alert-danger py-2 small mt-3 mb-0">{{ errorForm }}</div>',
        '        <div class="d-flex gap-2 mt-3">',
        '          <button type="submit" class="btn btn-sm btn-primary" :disabled="guardando">',
        '            {{ guardando ? "Guardando..." : (esEdicion ? "Guardar cambios" : "Crear cliente") }}',
        '          </button>',
        '          <button type="button" class="btn btn-sm btn-secondary" v-on:click="cancelarFormulario">Cancelar</button>',
        '        </div>',
        '      </form>',
        '    </div>',
        '  </div>',

        // ------- busqueda + listado -------
        '  <div class="row g-2 mb-3">',
        '    <div class="col-md-5">',
        '      <input v-model.trim="busqueda" class="form-control" placeholder="Buscar por nombre, apellido, correo o telefono ( AJAX )" />',
        '    </div>',
        '    <div class="col-auto d-flex align-items-center text-muted small">',
        '      {{ total }} cliente(s)',
        '    </div>',
        '  </div>',

        '  <div class="card shadow-sm">',
        '    <div class="table-responsive">',
        '      <table class="table table-hover align-middle mb-0">',
        '        <thead class="table-light">',
        '          <tr>',
        '            <th>Nombre</th>',
        '            <th>Telefono</th>',
        '            <th>Correo</th>',
        '            <th class="text-center">Estado</th>',
        '            <th class="text-end">Acciones</th>',
        '          </tr>',
        '        </thead>',
        '        <tbody>',
        '          <tr v-if="cargando"><td colspan="5" class="text-center text-muted py-4">Cargando clientes...</td></tr>',
        '          <tr v-else-if="errorCarga"><td colspan="5" class="text-center text-danger py-4">{{ errorCarga }}</td></tr>',
        '          <tr v-else-if="clientes.length === 0"><td colspan="5" class="text-center text-muted py-4">No hay clientes para mostrar.</td></tr>',
        '          <tr v-else v-for="cliente in clientes" :key="cliente.id" :class="{ \'table-secondary\': !cliente.estado }">',
        '            <td>{{ cliente.nombre }} {{ cliente.apellido }}</td>',
        '            <td>{{ cliente.telefono }}</td>',
        '            <td>{{ cliente.email }}</td>',
        '            <td class="text-center">',
        '              <span class="badge" :class="cliente.estado ? \'text-bg-success\' : \'text-bg-secondary\'">',
        '                {{ cliente.estado ? "Activo" : "Baja" }}',
        '              </span>',
        '            </td>',
        '            <td class="text-end">',
        '              <button type="button" class="btn btn-sm btn-outline-primary me-1" v-on:click="editar(cliente)">Editar</button>',
        '              <button type="button" class="btn btn-sm" v-on:click="cambiarEstado(cliente)"',
        '                      :class="cliente.estado ? \'btn-outline-danger\' : \'btn-outline-success\'">',
        '                {{ cliente.estado ? "Dar de baja" : "Reactivar" }}',
        '              </button>',
        '            </td>',
        '          </tr>',
        '        </tbody>',
        '      </table>',
        '    </div>',
        '  </div>',
        '</div>',

        '</div>'
    ].join("\n");

    Vue.createApp({
        template: plantilla,
        data: function () {
            return {
                // ------- sesion JWT -------
                token: sessionStorage.getItem(CLAVE_TOKEN) || "",
                usuarioActual: null,
                emailLogin: "",
                passwordLogin: "",
                errorLogin: "",
                logueando: false,

                // ------- abm -------
                clientes: [],
                total: 0,
                cargando: false,
                errorCarga: "",
                busqueda: "",
                temporizadorBusqueda: null,

                form: { id: 0, nombre: "", apellido: "", telefono: "", email: "", direccion: "" },
                guardando: false,
                errorForm: "",

                mensajeOk: ""
            };
        },

        computed: {
            esEdicion: function () {
                return this.form.id > 0;
            }
        },

        watch: {
            // Busqueda AJAX con debounce: consulta a la API 300 ms despues del ultimo tipeo.
            busqueda: function () {
                var app = this;
                clearTimeout(this.temporizadorBusqueda);
                this.temporizadorBusqueda = setTimeout(function () { app.listar(); }, 300);
            }
        },

        methods: {
            // ---------- base: fetch con Bearer ----------
            apiFetch: function (url, opciones) {
                var encabezados = { "Content-Type": "application/json" };
                if (this.token) {
                    encabezados["Authorization"] = "Bearer " + this.token;
                }
                return fetch(url, {
                    method: (opciones && opciones.metodo) || "GET",
                    headers: encabezados,
                    body: opciones && opciones.cuerpo ? JSON.stringify(opciones.cuerpo) : undefined
                }).then(function (respuesta) {
                    return respuesta
                        .json()
                        .catch(function () { return null; })
                        .then(function (datos) { return { ok: respuesta.ok, estado: respuesta.status, datos: datos }; });
                });
            },

            // ---------- sesion ----------
            ingresar: function () {
                var app = this;
                this.errorLogin = "";
                this.logueando = true;

                this.apiFetch("/api/auth/login", {
                    metodo: "POST",
                    cuerpo: { email: this.emailLogin, password: this.passwordLogin }
                }).then(function (respuesta) {
                    app.logueando = false;
                    if (!respuesta.ok || !respuesta.datos || !respuesta.datos.token) {
                        app.errorLogin = (respuesta.datos && respuesta.datos.mensaje) || "No se pudo iniciar sesion.";
                        return;
                    }
                    app.token = respuesta.datos.token;
                    app.usuarioActual = respuesta.datos.usuario;
                    app.passwordLogin = "";
                    sessionStorage.setItem(CLAVE_TOKEN, app.token);
                    app.listar();
                }).catch(function () {
                    app.logueando = false;
                    app.errorLogin = "Error de conexion con la API.";
                });
            },

            salir: function () {
                this.token = "";
                this.usuarioActual = null;
                this.clientes = [];
                this.total = 0;
                sessionStorage.removeItem(CLAVE_TOKEN);
            },

            // ---------- listado ----------
            listar: function () {
                var app = this;
                this.cargando = true;
                this.errorCarga = "";

                var consulta = "?pagina=1" + (this.busqueda ? "&busqueda=" + encodeURIComponent(this.busqueda) : "");

                this.apiFetch("/api/clientes" + consulta).then(function (respuesta) {
                    app.cargando = false;

                    if (respuesta.estado === 401) {
                        app.salir();
                        return;
                    }

                    if (!respuesta.ok || !respuesta.datos) {
                        app.errorCarga = "No se pudo obtener el listado de clientes.";
                        return;
                    }

                    app.clientes = respuesta.datos.clientes || [];
                    app.total = respuesta.datos.total || 0;
                }).catch(function () {
                    app.cargando = false;
                    app.errorCarga = "Error de conexion con la API.";
                });
            },

            // ---------- abm ----------
            prepararAlta: function () {
                this.form = { id: 0, nombre: "", apellido: "", telefono: "", email: "", direccion: "" };
                this.errorForm = "";
            },

            editar: function (cliente) {
                this.form = {
                    id: cliente.id,
                    nombre: cliente.nombre || "",
                    apellido: cliente.apellido || "",
                    telefono: cliente.telefono || "",
                    email: cliente.email || "",
                    direccion: cliente.direccion || ""
                };
                this.errorForm = "";
                window.scrollTo({ top: 0, behavior: "smooth" });
            },

            cancelarFormulario: function () {
                this.prepararAlta();
            },

            guardar: function () {
                var app = this;

                if (!this.form.nombre) {
                    this.errorForm = "El nombre es obligatorio.";
                    return;
                }
                if (this.form.email && this.form.email.indexOf("@") < 1) {
                    this.errorForm = "El correo ingresado no es valido.";
                    return;
                }

                this.errorForm = "";
                this.guardando = true;

                var cuerpo = {
                    nombre: this.form.nombre,
                    apellido: this.form.apellido,
                    telefono: this.form.telefono,
                    email: this.form.email,
                    direccion: this.form.direccion
                };

                var promesa = this.esEdicion
                    ? this.apiFetch("/api/clientes/" + this.form.id, { metodo: "PUT", cuerpo: cuerpo })
                    : this.apiFetch("/api/clientes", { metodo: "POST", cuerpo: cuerpo });

                promesa.then(function (respuesta) {
                    app.guardando = false;

                    if (respuesta.estado === 401) {
                        app.salir();
                        return;
                    }

                    if (!respuesta.ok) {
                        app.errorForm = (respuesta.datos && respuesta.datos.mensaje) || "No se pudo guardar el cliente.";
                        return;
                    }

                    app.mensajeOk = app.esEdicion
                        ? "Cliente actualizado correctamente."
                        : "Cliente creado correctamente.";
                    setTimeout(function () { app.mensajeOk = ""; }, 4000);

                    app.prepararAlta();
                    app.listar();
                }).catch(function () {
                    app.guardando = false;
                    app.errorForm = "Error de conexion con la API.";
                });
            },

            cambiarEstado: function (cliente) {
                var app = this;

                this.apiFetch("/api/clientes/" + cliente.id + "/estado", {
                    metodo: "PATCH",
                    cuerpo: { estado: !cliente.estado }
                }).then(function (respuesta) {
                    if (respuesta.estado === 401) {
                        app.salir();
                        return;
                    }

                    if (!respuesta.ok) {
                        app.errorCarga = (respuesta.datos && respuesta.datos.mensaje) || "No se pudo cambiar el estado.";
                        return;
                    }

                    app.listar();
                });
            }
        },

        mounted: function () {
            // Si hay token guardado, se valida contra la API antes de mostrar el ABM.
            var app = this;

            if (!this.token) {
                return;
            }

            this.apiFetch("/api/auth/perfil").then(function (respuesta) {
                if (!respuesta.ok) {
                    app.salir();
                    return;
                }
                app.usuarioActual = respuesta.datos;
                app.listar();
            }).catch(function () {
                app.salir();
            });
        }
    }).mount("#app-vue");
})();
