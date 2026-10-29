using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioInsumo : RepositorioBase, IRepositorioInsumo
{
    public RepositorioInsumo(IConfiguration configuration)
        : base(configuration)
    {
    }

    //------------------------------------------------------------LISTADO PAGINADO (CON BUSQUEDA) --------------------------------------------------------
    public ResultadoPaginado<Insumo> ObtenerTodos(int pagina, int tamano, string? busqueda = null, bool soloActivos = false)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<Insumo>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            var where = @"WHERE (@Busqueda = '' OR nombre LIKE CONCAT('%', @Busqueda, '%'))
            AND (@SoloActivos = 0 OR estado = 1)";

            string consultaSql = $@"
            SELECT id, nombre, unidad, costo_unitario, estado
            FROM insumo
            {where}
            ORDER BY estado DESC, nombre
            LIMIT @Offset, @Tamano;";

            using (var comando = new MySqlCommand(consultaSql, conexion))
            {
                CargarParametrosListado(comando, busqueda, offset, tamano, soloActivos);

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerInsumo(lector));
                }
            }

            using (var comando = new MySqlCommand($@"SELECT COUNT(*) FROM insumo {where};", conexion))
            {
                CargarParametrosListado(comando, busqueda, null, null, soloActivos);
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los insumos: {e.Message}");
        }

        return resultado;
    }

    //------------------------------------------------------------INSUMOS ACTIVOS (PARA RECETAS) --------------------------------------------------------
    public IList<Insumo> ObtenerActivos()
    {
        var lista = new List<Insumo>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, unidad, costo_unitario, estado
            FROM insumo
            WHERE estado = 1
            ORDER BY nombre;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                lista.Add(LeerInsumo(lector));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los insumos activos: {e.Message}");
        }

        return lista;
    }

    //------------------------------------------------------------OBTENER INSUMO POR ID --------------------------------------------------------
    public Insumo? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, unidad, costo_unitario, estado
            FROM insumo
            WHERE id = @Id
            LIMIT 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                return LeerInsumo(lector);
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener el insumo por ID: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------CREAR INSUMO --------------------------------------------------------
    public int CrearInsumo(Insumo insumo)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            INSERT INTO insumo (nombre, unidad, costo_unitario, estado)
            VALUES (@Nombre, @Unidad, @CostoUnitario, @Estado);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);

            comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = insumo.Nombre;
            comando.Parameters.Add("@Unidad", MySqlDbType.VarChar, 20).Value = insumo.Unidad.ToString();
            comando.Parameters.Add("@CostoUnitario", MySqlDbType.Decimal).Value = insumo.CostoUnitario;
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = insumo.Estado;

            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al crear el insumo: {e.Message}");
            return 0;
        }
    }

    //------------------------------------------------------------ACTUALIZAR INSUMO --------------------------------------------------------
    public bool ActualizarInsumo(Insumo insumo)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            UPDATE insumo
            SET nombre = @Nombre, unidad = @Unidad, costo_unitario = @CostoUnitario
            WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);

            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = insumo.Id;
            comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = insumo.Nombre;
            comando.Parameters.Add("@Unidad", MySqlDbType.VarChar, 20).Value = insumo.Unidad.ToString();
            comando.Parameters.Add("@CostoUnitario", MySqlDbType.Decimal).Value = insumo.CostoUnitario;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al actualizar el insumo: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------CAMBIAR ESTADO (BAJA LOGICA) --------------------------------------------------------
    public bool CambiarEstadoInsumo(int id, bool nuevoEstado)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"UPDATE insumo SET estado = @Estado WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = nuevoEstado;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al cambiar el estado del insumo: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static void CargarParametrosListado(MySqlCommand comando, string? busqueda, int? offset, int? tamano, bool soloActivos = false)
    {
        comando.Parameters.Add("@Busqueda", MySqlDbType.VarChar, 150).Value = busqueda?.Trim() ?? string.Empty;
        comando.Parameters.Add("@SoloActivos", MySqlDbType.Int32).Value = soloActivos ? 1 : 0;

        if (offset is not null)
        {
            comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset.Value;
        }

        if (tamano is not null)
        {
            comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano.Value;
        }
    }

    private static Insumo LeerInsumo(MySqlDataReader lector)
    {
        return new Insumo
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Unidad = Enum.Parse<UnidadMedida>(lector.GetString("unidad"), true),
            CostoUnitario = lector.GetDecimal("costo_unitario"),
            Estado = lector.GetBoolean("estado")
        };
    }
}
