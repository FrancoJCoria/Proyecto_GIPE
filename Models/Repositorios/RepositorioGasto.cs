using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioGasto : RepositorioBase, IRepositorioGasto
{
    public RepositorioGasto(IConfiguration configuration)
        : base(configuration)
    {
    }

    //------------------------------------------------------------LISTADO PAGINADO (CON FILTROS) --------------------------------------------------------
    public ResultadoPaginado<GastoListaItem> ObtenerTodos(
        int pagina, int tamano,
        DateTime? desde = null, DateTime? hasta = null, CategoriaGasto? categoria = null)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<GastoListaItem>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            string consultaSql = @"
            SELECT g.id, g.usuario_id, g.descripcion, g.categoria, g.monto, g.fecha,
                   u.nombre AS usuario_nombre
            FROM gasto g
            JOIN usuario u ON u.id = g.usuario_id
            WHERE (@Desde IS NULL OR g.fecha >= @Desde)
              AND (@Hasta IS NULL OR g.fecha <= @Hasta)
              AND (@Categoria IS NULL OR g.categoria = @Categoria)
            ORDER BY g.fecha DESC, g.id DESC
            LIMIT @Offset, @Tamano;";

            using (var comando = new MySqlCommand(consultaSql, conexion))
            {
                CargarParametrosListado(comando, desde, hasta, categoria, offset, tamano);

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerGastoListaItem(lector));
                }
            }

            string consultaTotal = @"
            SELECT COUNT(*)
            FROM gasto
            WHERE (@Desde IS NULL OR fecha >= @Desde)
              AND (@Hasta IS NULL OR fecha <= @Hasta)
              AND (@Categoria IS NULL OR categoria = @Categoria);";

            using (var comando = new MySqlCommand(consultaTotal, conexion))
            {
                CargarParametrosListado(comando, desde, hasta, categoria, null, null);
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los gastos: {e.Message}");
        }

        return resultado;
    }

    //------------------------------------------------------------OBTENER GASTO POR ID --------------------------------------------------------
    public Gasto? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, usuario_id, descripcion, categoria, monto, fecha
            FROM gasto
            WHERE id = @Id
            LIMIT 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                return new Gasto
                {
                    Id = lector.GetInt32("id"),
                    UsuarioId = lector.GetInt32("usuario_id"),
                    Descripcion = lector.GetString("descripcion"),
                    Categoria = Enum.Parse<CategoriaGasto>(lector.GetString("categoria"), true),
                    Monto = lector.GetDecimal("monto"),
                    Fecha = lector.GetDateTime("fecha")
                };
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener el gasto por ID: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------CREAR GASTO --------------------------------------------------------
    public int CrearGasto(Gasto gasto)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            INSERT INTO gasto (usuario_id, descripcion, categoria, monto, fecha)
            VALUES (@UsuarioId, @Descripcion, @Categoria, @Monto, @Fecha);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);
            CargarParametrosGasto(comando, gasto);

            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al crear el gasto: {e.Message}");
            return 0;
        }
    }

    //------------------------------------------------------------ACTUALIZAR GASTO --------------------------------------------------------
    public bool ActualizarGasto(Gasto gasto)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            // El usuario que registro el gasto no se toca en la edicion.
            string consultaSql = @"
            UPDATE gasto
            SET descripcion = @Descripcion, categoria = @Categoria, monto = @Monto, fecha = @Fecha
            WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = gasto.Id;
            comando.Parameters.Add("@Descripcion", MySqlDbType.VarChar, 200).Value = gasto.Descripcion;
            comando.Parameters.Add("@Categoria", MySqlDbType.VarChar, 20).Value = gasto.Categoria.ToString();
            comando.Parameters.Add("@Monto", MySqlDbType.Decimal).Value = gasto.Monto;
            comando.Parameters.Add("@Fecha", MySqlDbType.Date).Value = gasto.Fecha.Date;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al actualizar el gasto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------ELIMINAR GASTO --------------------------------------------------------
    public bool EliminarGasto(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"DELETE FROM gasto WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al eliminar el gasto: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static void CargarParametrosListado(
        MySqlCommand comando,
        DateTime? desde, DateTime? hasta, CategoriaGasto? categoria,
        int? offset, int? tamano)
    {
        comando.Parameters.Add("@Desde", MySqlDbType.Date).Value = (object?)desde?.Date ?? DBNull.Value;
        comando.Parameters.Add("@Hasta", MySqlDbType.Date).Value = (object?)hasta?.Date ?? DBNull.Value;
        comando.Parameters.Add("@Categoria", MySqlDbType.VarChar, 20).Value =
            categoria is null ? DBNull.Value : categoria.Value.ToString();

        if (offset is not null)
        {
            comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset.Value;
        }

        if (tamano is not null)
        {
            comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano.Value;
        }
    }

    private static void CargarParametrosGasto(MySqlCommand comando, Gasto gasto)
    {
        comando.Parameters.Add("@UsuarioId", MySqlDbType.Int32).Value = gasto.UsuarioId;
        comando.Parameters.Add("@Descripcion", MySqlDbType.VarChar, 200).Value = gasto.Descripcion;
        comando.Parameters.Add("@Categoria", MySqlDbType.VarChar, 20).Value = gasto.Categoria.ToString();
        comando.Parameters.Add("@Monto", MySqlDbType.Decimal).Value = gasto.Monto;
        comando.Parameters.Add("@Fecha", MySqlDbType.Date).Value = gasto.Fecha.Date;
    }

    private static GastoListaItem LeerGastoListaItem(MySqlDataReader lector)
    {
        return new GastoListaItem
        {
            Id = lector.GetInt32("id"),
            UsuarioNombre = lector.GetString("usuario_nombre"),
            Descripcion = lector.GetString("descripcion"),
            Categoria = Enum.Parse<CategoriaGasto>(lector.GetString("categoria"), true),
            Monto = lector.GetDecimal("monto"),
            Fecha = lector.GetDateTime("fecha")
        };
    }
}
