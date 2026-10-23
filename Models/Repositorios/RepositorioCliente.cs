using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioCliente : RepositorioBase, IRepositorioCliente
{
    public RepositorioCliente(IConfiguration configuration)
        : base(configuration)
    {
    }

    //------------------------------------------------------------LISTADO PAGINADO (CON BUSQUEDA) --------------------------------------------------------
    public ResultadoPaginado<Cliente> ObtenerTodos(int pagina, int tamano, string? busqueda = null)
    {
        pagina = Math.Max(1, pagina);
        var resultado = new ResultadoPaginado<Cliente>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            int offset = (pagina - 1) * tamano;

            var where = @"
            WHERE @Busqueda = ''
               OR nombre LIKE CONCAT('%', @Busqueda, '%')
               OR COALESCE(apellido, '') LIKE CONCAT('%', @Busqueda, '%')
               OR COALESCE(email, '') LIKE CONCAT('%', @Busqueda, '%')
               OR COALESCE(telefono, '') LIKE CONCAT('%', @Busqueda, '%')";

            string consultaSql = $@"
            SELECT id, nombre, apellido, telefono, email, direccion, estado
            FROM cliente
            {where}
            ORDER BY estado DESC, nombre
            LIMIT @Offset, @Tamano;";

            using (var comando = new MySqlCommand(consultaSql, conexion))
            {
                CargarParametrosListado(comando, busqueda, offset, tamano);

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerCliente(lector));
                }
            }

            using (var comando = new MySqlCommand($@"SELECT COUNT(*) FROM cliente {where};", conexion))
            {
                CargarParametrosListado(comando, busqueda, null, null);
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los clientes: {e.Message}");
        }

        return resultado;
    }

    //------------------------------------------------------------CLIENTES ACTIVOS (PARA DESPLEGABLES) --------------------------------------------------------
    public IList<Cliente> ObtenerActivos()
    {
        var lista = new List<Cliente>();

        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, apellido, telefono, email, direccion, estado
            FROM cliente
            WHERE estado = 1
            ORDER BY nombre;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                lista.Add(LeerCliente(lector));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener los clientes activos: {e.Message}");
        }

        return lista;
    }

    //------------------------------------------------------------OBTENER CLIENTE POR ID --------------------------------------------------------
    public Cliente? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"SELECT id, nombre, apellido, telefono, email, direccion, estado
            FROM cliente
            WHERE id = @Id
            LIMIT 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                return LeerCliente(lector);
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener el cliente por ID: {e.Message}");
            return null;
        }
    }

    //------------------------------------------------------------CREAR CLIENTE --------------------------------------------------------
    public int CrearCliente(Cliente cliente)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            INSERT INTO cliente (nombre, apellido, telefono, email, direccion, estado)
            VALUES (@Nombre, @Apellido, @Telefono, @Email, @Direccion, @Estado);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);

            comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = cliente.Nombre;
            comando.Parameters.Add("@Apellido", MySqlDbType.VarChar, 150).Value = (object?)cliente.Apellido ?? DBNull.Value;
            comando.Parameters.Add("@Telefono", MySqlDbType.VarChar, 30).Value = (object?)cliente.Telefono ?? DBNull.Value;
            comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = (object?)cliente.Email ?? DBNull.Value;
            comando.Parameters.Add("@Direccion", MySqlDbType.VarChar, 200).Value = (object?)cliente.Direccion ?? DBNull.Value;
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = cliente.Estado;

            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al crear el cliente: {e.Message}");
            return 0;
        }
    }

    //------------------------------------------------------------ACTUALIZAR CLIENTE --------------------------------------------------------
    public bool ActualizarCliente(Cliente cliente)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"
            UPDATE cliente
            SET nombre = @Nombre, apellido = @Apellido, telefono = @Telefono,
                email = @Email, direccion = @Direccion
            WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);

            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = cliente.Id;
            comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = cliente.Nombre;
            comando.Parameters.Add("@Apellido", MySqlDbType.VarChar, 150).Value = (object?)cliente.Apellido ?? DBNull.Value;
            comando.Parameters.Add("@Telefono", MySqlDbType.VarChar, 30).Value = (object?)cliente.Telefono ?? DBNull.Value;
            comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = (object?)cliente.Email ?? DBNull.Value;
            comando.Parameters.Add("@Direccion", MySqlDbType.VarChar, 200).Value = (object?)cliente.Direccion ?? DBNull.Value;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al actualizar el cliente: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------CAMBIAR ESTADO (BAJA LOGICA) --------------------------------------------------------
    public bool CambiarEstadoCliente(int id, bool nuevoEstado)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            string consultaSql = @"UPDATE cliente SET estado = @Estado WHERE id = @Id;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = nuevoEstado;

            return comando.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al cambiar el estado del cliente: {e.Message}");
            return false;
        }
    }

    //------------------------------------------------------------HISTORIAL DE COMPRAS DEL CLIENTE --------------------------------------------------------
    public ResultadoPaginado<VentaListaItem> ObtenerCompras(int clienteId, int pagina, int tamano)
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
            WHERE v.cliente_id = @ClienteId
            ORDER BY v.fecha DESC, v.id DESC
            LIMIT @Offset, @Tamano;", conexion))
            {
                comando.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = clienteId;
                comando.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset;
                comando.Parameters.Add("@Tamano", MySqlDbType.Int32).Value = tamano;

                using var lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    resultado.Filas.Add(LeerVentaListaItem(lector));
                }
            }

            using (var comando = new MySqlCommand(@"
            SELECT COUNT(*) FROM venta WHERE cliente_id = @ClienteId;", conexion))
            {
                comando.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = clienteId;
                resultado.Total = Convert.ToInt32(comando.ExecuteScalar());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al obtener las compras del cliente: {e.Message}");
        }

        return resultado;
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

    private static Cliente LeerCliente(MySqlDataReader lector)
    {
        int indiceApellido = lector.GetOrdinal("apellido");
        int indiceTelefono = lector.GetOrdinal("telefono");
        int indiceEmail = lector.GetOrdinal("email");
        int indiceDireccion = lector.GetOrdinal("direccion");

        return new Cliente
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Apellido = lector.IsDBNull(indiceApellido) ? null : lector.GetString(indiceApellido),
            Telefono = lector.IsDBNull(indiceTelefono) ? null : lector.GetString(indiceTelefono),
            Email = lector.IsDBNull(indiceEmail) ? null : lector.GetString(indiceEmail),
            Direccion = lector.IsDBNull(indiceDireccion) ? null : lector.GetString(indiceDireccion),
            Estado = lector.GetBoolean("estado")
        };
    }

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
}
