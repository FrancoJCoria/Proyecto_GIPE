using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioProducto : RepositorioBase, IRepositorioProducto
{
    public RepositorioProducto(IConfiguration configuration)
        : base(configuration)
    {
    }

    //------------------------------------------------------------LISTADO PAGINADO (CON BUSQUEDA) --------------------------------------------------------
    public ResultadoPaginado<Producto> ObtenerTodos(int pagina, int tamano, string? busqueda = null)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<Producto>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            var where = @"WHERE @Busqueda = '' OR nombre LIKE CONCAT('%', @Busqueda, '%')";

            string consultaSql = $@"
            SELECT id, nombre, descripcion, costo_extra, margen_objetivo, precio_actual, stock_actual, url_producto, estado
            FROM producto
            {where}
            ORDER BY estado DESC, nombre
            LIMIT @Offset, @Tamano;";

            using (var comando = new MySqlCommand(consultaSql, conexion))
            {
                CargarParametrosListado(comando, busqueda, offset, tamano);

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerProducto(lector));
                }
            }

            using (var comando = new MySqlCommand($@"SELECT COUNT(*) FROM producto {where};", conexion))
            {
                CargarParametrosListado(comando, busqueda, null, null);
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los productos: {e.Message}");
        }

        return resultado;
    }

    //------------------------------------------------------------PRODUCTOS ACTIVOS (PARA DESPLEGABLES) --------------------------------------------------------
    public IList<Producto> ObtenerActivos()
    {
        var lista = new List<Producto>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, descripcion, costo_extra, margen_objetivo, precio_actual, stock_actual, url_producto, estado
            FROM producto
            WHERE estado = 1
            ORDER BY nombre;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                lista.Add(LeerProducto(lector));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los productos activos: {e.Message}");
        }

        return lista;
    }

    //------------------------------------------------------------OBTENER PRODUCTO POR ID --------------------------------------------------------
    public Producto? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, descripcion, costo_extra, margen_objetivo, precio_actual, stock_actual, url_producto, estado
            FROM producto
            WHERE id = @Id
            LIMIT 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                return LeerProducto(lector);
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener el producto por ID: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------CREAR PRODUCTO --------------------------------------------------------
    public int CrearProducto(Producto producto)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            INSERT INTO producto (nombre, descripcion, costo_extra, margen_objetivo, precio_actual, stock_actual, url_producto, estado)
            VALUES (@Nombre, @Descripcion, @CostoExtra, @MargenObjetivo, @PrecioActual, @StockActual, @UrlProducto, @Estado);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);

            CargarParametrosProducto(comando, producto, incluirEstado: true);

            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al crear el producto: {e.Message}");
            return 0;
        }
    }

    //------------------------------------------------------------ACTUALIZAR PRODUCTO (STOCK MANUAL) --------------------------------------------------------
    public bool ActualizarProducto(Producto producto)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            UPDATE producto
            SET nombre = @Nombre, descripcion = @Descripcion, costo_extra = @CostoExtra,
                margen_objetivo = @MargenObjetivo, precio_actual = @PrecioActual,
                stock_actual = @StockActual, url_producto = @UrlProducto
            WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);

            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = producto.Id;
            CargarParametrosProducto(comando, producto, incluirEstado: false);

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al actualizar el producto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------CAMBIAR ESTADO (BAJA LOGICA) --------------------------------------------------------
    public bool CambiarEstadoProducto(int id, bool nuevoEstado)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"UPDATE producto SET estado = @Estado WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = nuevoEstado;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al cambiar el estado del producto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------CARGA MANUAL DE STOCK --------------------------------------------------------
    public bool ActualizarStock(int id, decimal stock)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            UPDATE producto
            SET stock_actual = @Stock
            WHERE id = @Id AND @Stock >= 0;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            comando.Parameters.Add("@Stock", MySqlDbType.Decimal).Value = stock;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al actualizar el stock del producto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------COSTO DE PRODUCCION (RECETA + COSTO EXTRA) --------------------------------------------------------
    public decimal? ObtenerCostoProducto(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            SELECT p.costo_extra + COALESCE(SUM(i.costo_unitario * pi.cantidad), 0) AS costo_total
            FROM producto p
            LEFT JOIN producto_insumo pi ON pi.producto_id = p.id
            LEFT JOIN insumo i ON i.id = pi.insumo_id
            WHERE p.id = @Id
            GROUP BY p.id, p.costo_extra;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            var valor = comando.ExecuteScalar();

            if (valor is null or DBNull)
            {
                return null;
            }

            return Convert.ToDecimal(valor);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al calcular el costo del producto: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------RECETA DEL PRODUCTO --------------------------------------------------------
    public IList<RecetaItem> ObtenerReceta(int productoId)
    {
        var lista = new List<RecetaItem>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            SELECT pi.insumo_id, i.nombre, i.unidad, i.costo_unitario, pi.cantidad
            FROM producto_insumo pi
            JOIN insumo i ON i.id = pi.insumo_id
            WHERE pi.producto_id = @ProductoId
            ORDER BY i.nombre;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = productoId;

            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                int indiceCantidad = lector.GetOrdinal("cantidad");

                lista.Add(new RecetaItem
                {
                    InsumoId = lector.GetInt32("insumo_id"),
                    Nombre = lector.GetString("nombre"),
                    Unidad = Enum.Parse<UnidadMedida>(lector.GetString("unidad"), true),
                    CostoUnitario = lector.GetDecimal("costo_unitario"),
                    Cantidad = lector.IsDBNull(indiceCantidad) ? 0 : lector.GetDecimal(indiceCantidad)
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener la receta del producto: {e.Message}");
        }

        return lista;
    }

    //------------------------------------------------------------GUARDAR RECETA (REEMPLAZO COMPLETO, EN TRANSACCION) --------------------------------------------------------
    public bool GuardarReceta(int productoId, IList<ProductoInsumo> lineas)
    {
        // Si un mismo insumo viene repetido, se queda la ultima cantidad indicada.
        var unicas = lineas
            .GroupBy(l => l.InsumoId)
            .Select(g => g.Last())
            .Where(l => l.Cantidad > 0)
            .ToList();

        using var conexion = new MySqlConnection(connectionString);

        try
        {
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using (var borrar = new MySqlCommand(
                @"DELETE FROM producto_insumo WHERE producto_id = @ProductoId;", conexion, transaccion))
            {
                borrar.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = productoId;
                borrar.ExecuteNonQuery();
            }

            foreach (var linea in unicas)
            {
                using var insertar = new MySqlCommand(@"
                INSERT INTO producto_insumo (producto_id, insumo_id, cantidad)
                VALUES (@ProductoId, @InsumoId, @Cantidad);", conexion, transaccion);

                insertar.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = productoId;
                insertar.Parameters.Add("@InsumoId", MySqlDbType.Int32).Value = linea.InsumoId;
                insertar.Parameters.Add("@Cantidad", MySqlDbType.Decimal).Value = linea.Cantidad;
                insertar.ExecuteNonQuery();
            }

            transaccion.Commit();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al guardar la receta del producto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------QUITAR UNA LINEA DE LA RECETA --------------------------------------------------------
    public bool EliminarLineaReceta(int productoId, int insumoId)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            DELETE FROM producto_insumo
            WHERE producto_id = @ProductoId AND insumo_id = @InsumoId;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@ProductoId", MySqlDbType.Int32).Value = productoId;
            comando.Parameters.Add("@InsumoId", MySqlDbType.Int32).Value = insumoId;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al quitar el insumo de la receta: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static void CargarParametrosListado(MySqlCommand comando, string? busqueda, int? offset, int? tamano)
    {
        comando.Parameters.Add("@Busqueda", MySqlDbType.VarChar, 150).Value = busqueda?.Trim() ?? string.Empty;

        if (offset is not null)
        {
            comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset.Value;
        }

        if (tamano is not null)
        {
            comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano.Value;
        }
    }

    private static void CargarParametrosProducto(MySqlCommand comando, Producto producto, bool incluirEstado)
    {
        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = producto.Nombre;
        comando.Parameters.Add("@Descripcion", MySqlDbType.VarChar, 500).Value = (object?)producto.Descripcion ?? DBNull.Value;
        comando.Parameters.Add("@CostoExtra", MySqlDbType.Decimal).Value = producto.CostoExtra;
        comando.Parameters.Add("@MargenObjetivo", MySqlDbType.Decimal).Value = producto.MargenObjetivo;
        comando.Parameters.Add("@PrecioActual", MySqlDbType.Decimal).Value = producto.PrecioActual;
        comando.Parameters.Add("@StockActual", MySqlDbType.Decimal).Value = producto.StockActual;
        comando.Parameters.Add("@UrlProducto", MySqlDbType.VarChar, 500).Value = (object?)producto.UrlProducto ?? DBNull.Value;

        if (incluirEstado)
        {
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = producto.Estado;
        }
    }

    private static Producto LeerProducto(MySqlDataReader lector)
    {
        int indiceDescripcion = lector.GetOrdinal("descripcion");
        int indiceUrl = lector.GetOrdinal("url_producto");

        return new Producto
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Descripcion = lector.IsDBNull(indiceDescripcion) ? null : lector.GetString(indiceDescripcion),
            CostoExtra = lector.GetDecimal("costo_extra"),
            MargenObjetivo = lector.GetDecimal("margen_objetivo"),
            PrecioActual = lector.GetDecimal("precio_actual"),
            StockActual = lector.GetDecimal("stock_actual"),
            UrlProducto = lector.IsDBNull(indiceUrl) ? null : lector.GetString(indiceUrl),
            Estado = lector.GetBoolean("estado")
        };
    }
}
