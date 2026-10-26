using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.Excepciones;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioVenta : RepositorioBase, IRepositorioVenta
{
    public RepositorioVenta(IConfiguration configuration)
        : base(configuration)
    {
    }

    //------------------------------------------------------------LISTADO PAGINADO --------------------------------------------------------
    public ResultadoPaginado<VentaListaItem> ObtenerTodos(int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<VentaListaItem>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            using (var comando = new MySqlCommand(@"
            SELECT v.id, v.fecha, v.monto_total, v.estado,
                   c.nombre AS cliente_nombre,
                   u.nombre AS usuario_nombre
            FROM venta v
            JOIN usuario u ON u.id = v.usuario_id
            LEFT JOIN cliente c ON c.id = v.cliente_id
            ORDER BY v.fecha DESC, v.id DESC
            LIMIT @Offset, @Tamano;", conexion))
            {
                comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset;
                comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano;

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerVentaListaItem(lector));
                }
            }

            using (var comando = new MySqlCommand(@"SELECT COUNT(*) FROM venta;", conexion))
            {
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener las ventas: {e.Message}");
        }

        return resultado;
    }

    //------------------------------------------------------------DETALLE DE VENTA --------------------------------------------------------
    public VentaDetalleViewModel? ObtenerDetalle(int ventaId)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            var detalle = new VentaDetalleViewModel();

            string consultaVenta = @"
            SELECT v.id, v.fecha, v.cliente_id, v.usuario_id, v.monto_total, v.estado,
                   c.nombre AS cliente_nombre,
                   u.nombre AS usuario_nombre
            FROM venta v
            JOIN usuario u ON u.id = v.usuario_id
            LEFT JOIN cliente c ON c.id = v.cliente_id
            WHERE v.id = @VentaId
            LIMIT 1;";

            using (var comando = new MySqlCommand(consultaVenta, conexion))
            {
                comando.Parameters.Add("@VentaId", MySqlDbType.Int32).Value = ventaId;

                using var lector = comando.ExecuteReader();

                if (!lector.Read())
                {
                    return null;
                }

                int indiceCliente = lector.GetOrdinal("cliente_id");

                detalle.Venta = new Venta
                {
                    Id = lector.GetInt32("id"),
                    Fecha = lector.GetDateTime("fecha"),
                    ClienteId = lector.IsDBNull(indiceCliente) ? null : lector.GetInt32(indiceCliente),
                    UsuarioId = lector.GetInt32("usuario_id"),
                    MontoTotal = lector.GetDecimal("monto_total"),
                    Estado = lector.GetBoolean("estado")
                };

                detalle.UsuarioNombre = lector.GetString("usuario_nombre");

                int indiceClienteNombre = lector.GetOrdinal("cliente_nombre");
                detalle.ClienteNombre = lector.IsDBNull(indiceClienteNombre) ? null : lector.GetString(indiceClienteNombre);
            }

            string consultaItems = @"
            SELECT vi.producto_id, vi.cantidad, vi.precio_momento, vi.costo_momento, p.nombre
            FROM venta_item vi
            JOIN producto p ON p.id = vi.producto_id
            WHERE vi.venta_id = @VentaId
            ORDER BY vi.id;";

            using (var comando = new MySqlCommand(consultaItems, conexion))
            {
                comando.Parameters.Add("@VentaId", MySqlDbType.Int32).Value = ventaId;

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    detalle.Items.Add(new ItemVentaDetalle
                    {
                        ProductoId = lector.GetInt32("producto_id"),
                        ProductoNombre = lector.GetString("nombre"),
                        Cantidad = lector.GetDecimal("cantidad"),
                        PrecioMomento = lector.GetDecimal("precio_momento"),
                        CostoMomento = lector.GetDecimal("costo_momento")
                    });
                }
            }

            return detalle;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener el detalle de la venta: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------CREAR VENTA (SNAPSHOT + DESCUENTO DE STOCK, EN TRANSACCION) --------------------------------------------------------
    public int CrearVenta(int? clienteId, int usuarioId, IList<LineaVenta> lineas)
    {
        if (lineas.Count == 0)
        {
            throw new ReglaDeNegocioException("La venta no tiene items: agrega al menos un producto.");
        }

        MySqlConnection? conexion = null;
        MySqlTransaction? transaccion = null;

        try
        {
            conexion = new MySqlConnection(connectionString);
            conexion.Open();
            transaccion = conexion.BeginTransaction();

            // El usuario que registra la venta debe existir y estar activo.
            if (!ExisteActivo(conexion, transaccion, "usuario", usuarioId))
            {
                throw new ReglaDeNegocioException("El usuario que registra la venta no existe o esta inactivo.");
            }

            // Cliente opcional: si se cargo un id, debe existir y estar activo.
            if (clienteId is not null && !ExisteActivo(conexion, transaccion, "cliente", clienteId.Value))
            {
                throw new ReglaDeNegocioException("El cliente seleccionado no existe o esta inactivo.");
            }

            int ventaId;
            using (var comandoVenta = new MySqlCommand(@"
                INSERT INTO venta (cliente_id, usuario_id, fecha, monto_total, estado)
                VALUES (@ClienteId, @UsuarioId, NOW(), 0, 1);
                SELECT LAST_INSERT_ID();", conexion, transaccion))
            {
                comandoVenta.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = (object?)clienteId ?? DBNull.Value;
                comandoVenta.Parameters.Add("@UsuarioId", MySqlDbType.Int32).Value = usuarioId;
                ventaId = Convert.ToInt32(comandoVenta.ExecuteScalar());
            }

            decimal total = 0;

            foreach (var linea in lineas)
            {
                if (linea.ProductoId <= 0 || linea.Cantidad <= 0)
                {
                    throw new ReglaDeNegocioException("Hay items sin producto o con cantidad invalida.");
                }

                // Traer el producto con candado FOR UPDATE para que dos ventas simultaneas
                // no vendan el mismo stock. Precio y costo se congelan en este momento.
                decimal precioMomento;
                decimal costoMomento;
                decimal stockDisponible;
                string nombreProducto;

                using (var comandoProducto = new MySqlCommand(@"
                    SELECT p.nombre, p.precio_actual, p.stock_actual,
                           COALESCE((SELECT SUM(i.costo_unitario * pi.cantidad)
                                     FROM producto_insumo pi
                                     JOIN insumo i ON i.id = pi.insumo_id
                                     WHERE pi.producto_id = p.id), 0) + p.costo_extra AS costo_actual
                    FROM producto p
                    WHERE p.id = @ProductoId AND p.estado = 1
                    FOR UPDATE;", conexion, transaccion))
                {
                    comandoProducto.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = linea.ProductoId;

                    using var lector = comandoProducto.ExecuteReader();

                    if (!lector.Read())
                    {
                        throw new ReglaDeNegocioException(
                            $"El producto id {linea.ProductoId} no existe o esta inactivo.");
                    }

                    nombreProducto = lector.GetString("nombre");
                    precioMomento = lector.GetDecimal("precio_actual");
                    stockDisponible = lector.GetDecimal("stock_actual");
                    costoMomento = lector.GetDecimal("costo_actual");
                }

                if (stockDisponible < linea.Cantidad)
                {
                    throw new StockInsuficienteException(nombreProducto, stockDisponible, linea.Cantidad);
                }

                using (var comandoItem = new MySqlCommand(@"
                    INSERT INTO venta_item (venta_id, producto_id, cantidad, precio_momento, costo_momento)
                    VALUES (@VentaId, @ProductoId, @Cantidad, @PrecioMomento, @CostoMomento);", conexion, transaccion))
                {
                    comandoItem.Parameters.Add("@VentaId", MySqlDbType.Int32).Value = ventaId;
                    comandoItem.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = linea.ProductoId;
                    comandoItem.Parameters.Add("@Cantidad", MySqlDbType.Decimal).Value = linea.Cantidad;
                    comandoItem.Parameters.Add("@PrecioMomento", MySqlDbType.Decimal).Value = precioMomento;
                    comandoItem.Parameters.Add("@CostoMomento", MySqlDbType.Decimal).Value = costoMomento;
                    comandoItem.ExecuteNonQuery();
                }

                using (var comandoStock = new MySqlCommand(@"
                    UPDATE producto
                    SET stock_actual = stock_actual - @Cantidad
                    WHERE id = @ProductoId;", conexion, transaccion))
                {
                    comandoStock.Parameters.Add("@Cantidad", MySqlDbType.Decimal).Value = linea.Cantidad;
                    comandoStock.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = linea.ProductoId;

                    if (comandoStock.ExecuteNonQuery() != 1)
                    {
                        throw new ReglaDeNegocioException($"No se pudo descontar el stock de '{nombreProducto}'.");
                    }
                }

                total += linea.Cantidad * precioMomento;
            }

            // Grabar el monto total calculado con los precios del momento.
            using (var comandoTotal = new MySqlCommand(@"
                UPDATE venta SET monto_total = @Total WHERE id = @VentaId;", conexion, transaccion))
            {
                comandoTotal.Parameters.Add("@Total", MySqlDbType.Decimal).Value = total;
                comandoTotal.Parameters.Add("@VentaId", MySqlDbType.Int32).Value = ventaId;
                comandoTotal.ExecuteNonQuery();
            }

            transaccion.Commit();
            return ventaId;
        }
        catch (Exception e)
        {
            try
            {
                transaccion?.Rollback();
            }
            catch (Exception eRollback)
            {
                Console.WriteLine($"Error al deshacer la transaccion de la venta: {eRollback.Message}");
            }

            // Las reglas de negocio suben tal cual (el controlador muestra el mensaje);
            // los errores de base de datos se registran y se relanzan para no ocultarlos.
            if (e is not ReglaDeNegocioException)
            {
                Console.WriteLine($"Error al crear la venta: {e.Message}");
            }

            throw;
        }
        finally
        {
            transaccion?.Dispose();
            conexion?.Dispose();
        }
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static VentaListaItem LeerVentaListaItem(MySqlDataReader lector)
    {
        int indiceCliente = lector.GetOrdinal("cliente_nombre");

        return new VentaListaItem
        {
            Id = lector.GetInt32("id"),
            Fecha = lector.GetDateTime("fecha"),
            ClienteNombre = lector.IsDBNull(indiceCliente) ? null : lector.GetString(indiceCliente),
            UsuarioNombre = lector.GetString("usuario_nombre"),
            MontoTotal = lector.GetDecimal("monto_total"),
            Estado = lector.GetBoolean("estado")
        };
    }

    private static bool ExisteActivo(MySqlConnection conexion, MySqlTransaction transaccion, string tabla, int id)
    {
        // El nombre de tabla se arma aca con valores fijos internos ("usuario"/"cliente"); no viene
        // de entrada del usuario, por lo que no hay riesgo de inyeccion SQL.
        using var comando = new MySqlCommand(
            $"SELECT COUNT(*) FROM {tabla} WHERE id = @Id AND estado = 1;", conexion, transaccion);
        comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
        return Convert.ToInt64(comando.ExecuteScalar()) > 0;
    }
}
