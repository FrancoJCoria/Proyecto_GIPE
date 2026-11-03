// ABM de productos con Vue 3. Todo va por fetch (AJAX/JSON) contra /api/productos, sin recargar la pagina.
const { createApp } = Vue;

const TAMANO = 10;

// Si el servidor responde error, devuelve el mensaje en texto; si no, el JSON o un null.
async function pedir(url, opciones) {
    const resp = await fetch(url, {
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        ...opciones
    });
    if (!resp.ok) {
        const datos = await resp.json().catch(() => ({}));
        const detalle = datos.errors ? Object.values(datos.errors).flat().join(' ') : datos.mensaje;
        throw new Error(detalle || `Error ${resp.status}`);
    }
    return resp.status === 204 ? null : resp.json();
}

createApp({
    data() {
        return {
            esDueno: document.getElementById('app').dataset.esDueno === 'true',
            productos: [],
            pagina: 1,
            total: 0,
            busqueda: '',
            form: null,  
            error: ''
        };
    },
    computed: {
        totalPaginas() { return Math.max(1, Math.ceil(this.total / TAMANO)); }
    },
    mounted() { this.cargar(); },
    methods: {
        //LISTAR: pide solo la pagina actual al servidor
        async cargar() {
            try {
                const q = new URLSearchParams({ pagina: this.pagina, tamano: TAMANO, busqueda: this.busqueda });
                const datos = await pedir(`/api/productos?${q}`);
                this.productos = datos.productos;
                this.total = datos.total;
                this.error = '';
            } catch (e) { this.error = e.message; }
        },
        buscar() { this.pagina = 1; this.cargar(); },
        irAPagina(n) { this.pagina = n; this.cargar(); },

        nuevo() {
            this.form = { id: 0, nombre: '', descripcion: '', costoExtra: 0, margenObjetivo: 30, precioActual: 0, stockActual: 0 };
        },
        editar(p) { this.form = { ...p }; },

        //ALTA o MODIFICACION segun tenga id o no
        async guardar() {
            try {
                const editando = this.form.id > 0;
                await pedir(editando ? `/api/productos/${this.form.id}` : '/api/productos', {
                    method: editando ? 'PUT' : 'POST',
                    body: JSON.stringify(this.form)
                });
                this.form = null;
                await this.cargar();
            } catch (e) { this.error = e.message; }
        },

        //BAJA logica 
        async baja(p) {
            if (!confirm(`¿Dar de baja "${p.nombre}"?`)) return;
            try {
                await pedir(`/api/productos/${p.id}`, { method: 'DELETE' });
                await this.cargar();
            } catch (e) { this.error = e.message; }
        }
    }
}).mount('#app');
