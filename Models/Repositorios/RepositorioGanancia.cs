using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioGanancia : RepositorioBase, IRepositorioGanancia
{
    public RepositorioGanancia(IConfiguration configuration)
        : base(configuration)
    {
    }

    public decimal ObtenerIngresos(DateTime desde, DateTime hasta)
    {
        string consultaSql = @"
        SELECT COALESCE(SUM(monto_total), 0)
        FROM venta
        WHERE estado = 1
          AND DATE(fecha) >= @Desde
          AND DATE(fecha) <= @Hasta;";

        return ObtenerTotal(consultaSql, desde, hasta);
    }

    public decimal ObtenerCostoVentas(DateTime desde, DateTime hasta)
    {
        string consultaSql = @"
        SELECT COALESCE(SUM(vi.cantidad * vi.costo_momento), 0)
        FROM venta_item vi
        JOIN venta v ON v.id = vi.venta_id
        WHERE v.estado = 1
          AND DATE(v.fecha) >= @Desde
          AND DATE(v.fecha) <= @Hasta;";

        return ObtenerTotal(consultaSql, desde, hasta);
    }

    public decimal ObtenerGastos(DateTime desde, DateTime hasta)
    {
        string consultaSql = @"
        SELECT COALESCE(SUM(monto), 0)
        FROM gasto
        WHERE fecha >= @Desde
          AND fecha <= @Hasta;";

        return ObtenerTotal(consultaSql, desde, hasta);
    }

    public ResultadoPaginado<RentabilidadProducto> ObtenerRentabilidadPorProducto(
        DateTime desde, DateTime hasta, int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<RentabilidadProducto>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            string consultaSql = @"
            SELECT p.id, p.nombre,
                   SUM(vi.cantidad) AS cantidad_vendida,
                   SUM(vi.cantidad * vi.precio_momento) AS ingresos,
                   SUM(vi.cantidad * vi.costo_momento) AS costo,
                   SUM(vi.cantidad * vi.precio_momento) - SUM(vi.cantidad * vi.costo_momento) AS ganancia
            FROM venta_item vi
            JOIN venta v ON v.id = vi.venta_id
            JOIN producto p ON p.id = vi.producto_id
            WHERE v.estado = 1
              AND DATE(v.fecha) >= @Desde
              AND DATE(v.fecha) <= @Hasta
            GROUP BY p.id, p.nombre
            ORDER BY ganancia DESC, p.nombre
            LIMIT @Offset, @Tamano;";

            using (var comando = new MySqlCommand(consultaSql, conexion))
            {
                CargarFechas(comando, desde, hasta);
                comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset;
                comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano;

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(new RentabilidadProducto
                    {
                        ProductoId = lector.GetInt32("id"),
                        Nombre = lector.GetString("nombre"),
                        CantidadVendida = lector.GetDecimal("cantidad_vendida"),
                        Ingresos = lector.GetDecimal("ingresos"),
                        Costo = lector.GetDecimal("costo"),
                        Ganancia = lector.GetDecimal("ganancia")
                    });
                }
            }

            string consultaTotal = @"
            SELECT COUNT(DISTINCT vi.producto_id)
            FROM venta_item vi
            JOIN venta v ON v.id = vi.venta_id
            WHERE v.estado = 1
              AND DATE(v.fecha) >= @Desde
              AND DATE(v.fecha) <= @Hasta;";

            using (var comando = new MySqlCommand(consultaTotal, conexion))
            {
                CargarFechas(comando, desde, hasta);
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener la rentabilidad por producto: {e.Message}");
        }

        return resultado;
    }

    private decimal ObtenerTotal(string consultaSql, DateTime desde, DateTime hasta)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            using var comando = new MySqlCommand(consultaSql, conexion);
            CargarFechas(comando, desde, hasta);

            return Convert.ToDecimal(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al calcular el total del panel de ganancias: {e.Message}");
            return 0;
        }
    }

    private static void CargarFechas(MySqlCommand comando, DateTime desde, DateTime hasta)
    {
        comando.Parameters.Add("@Desde", MySqlDbType.Date).Value = desde.Date;
        comando.Parameters.Add("@Hasta", MySqlDbType.Date).Value = hasta.Date;
    }
}
