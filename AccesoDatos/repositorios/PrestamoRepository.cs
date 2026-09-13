using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Repositorios;

public class PrestamoRepository(
    ISqlConnectionFactory conexionFactory,
    IUsuarioRepository usuarioRepository,
    ILibroRepository libroRepository) : IPrestamoRepository
{
    private readonly ISqlConnectionFactory _conexionFactory = conexionFactory ?? throw new ArgumentNullException(nameof(conexionFactory));
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    private readonly ILibroRepository _libroRepository = libroRepository ?? throw new ArgumentNullException(nameof(libroRepository));

    public async Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync()
    {
        var filas = await LeerFilasPrestamoAsync("SELECT ID_Prestamo, ID_Usuario, Fecha_Prestamo, Fecha_Devolucion_Esperada, Estado FROM Prestamos", null);
        return await ReconstruirTodosAsync(filas);
    }

    public async Task<IReadOnlyList<Prestamo>> ObtenerPorUsuarioAsync(int idUsuario)
    {
        var filas = await LeerFilasPrestamoAsync(
            "SELECT ID_Prestamo, ID_Usuario, Fecha_Prestamo, Fecha_Devolucion_Esperada, Estado FROM Prestamos WHERE ID_Usuario = @IdUsuario",
            comando => comando.Parameters.AddWithValue("@IdUsuario", idUsuario));
        return await ReconstruirTodosAsync(filas);
    }

    public async Task<Prestamo?> ObtenerPorIdAsync(int idPrestamo)
    {
        var filas = await LeerFilasPrestamoAsync(
            "SELECT ID_Prestamo, ID_Usuario, Fecha_Prestamo, Fecha_Devolucion_Esperada, Estado FROM Prestamos WHERE ID_Prestamo = @IdPrestamo",
            comando => comando.Parameters.AddWithValue("@IdPrestamo", idPrestamo));

        if (filas.Count == 0)
            return null;

        var (id, idUsuario, fechaPrestamo, fechaDevolucion, estado) = filas[0];
        return await ReconstruirPrestamoAsync(id, idUsuario, fechaPrestamo, fechaDevolucion, estado);
    }

    public async Task<int> RegistrarAsync(Prestamo prestamo)
    {
        ArgumentNullException.ThrowIfNull(prestamo);
        if (prestamo.Detalles.Count == 0)
            throw new InvalidOperationException("Un préstamo debe incluir al menos un libro.");

        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            var idPrestamo = await InsertarPrestamoAsync(conexion, transaccion, prestamo);

            foreach (var detalle in prestamo.Detalles)
                await InsertarDetalleYActualizarLibroAsync(conexion, transaccion, idPrestamo, detalle);

            await transaccion.CommitAsync();
            return idPrestamo;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public async Task ActualizarEstadoAsync(Prestamo prestamo)
    {
        ArgumentNullException.ThrowIfNull(prestamo);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var comandoEstado = new SqlCommand(
                "UPDATE Prestamos SET Estado = @Estado WHERE ID_Prestamo = @IdPrestamo", conexion, transaccion))
            {
                comandoEstado.Parameters.AddWithValue("@Estado", prestamo.Estado.ToString());
                comandoEstado.Parameters.AddWithValue("@IdPrestamo", prestamo.IdPrestamo);
                var filasAfectadas = await comandoEstado.ExecuteNonQueryAsync();
                if (filasAfectadas == 0)
                    throw new InvalidOperationException($"No existe un préstamo con el id {prestamo.IdPrestamo}.");
            }

            if (prestamo.Estado == EstadoPrestamo.Devuelto)
            {
                foreach (var detalle in prestamo.Detalles)
                {
                    using var comandoLibro = new SqlCommand(
                        "UPDATE Libros SET Disponible = Disponible + @Cantidad WHERE ID_Libro = @IdLibro",
                        conexion, transaccion);
                    comandoLibro.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    comandoLibro.Parameters.AddWithValue("@IdLibro", detalle.Libro.IdLibro);
                    await comandoLibro.ExecuteNonQueryAsync();
                }
            }

            await transaccion.CommitAsync();
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> InsertarPrestamoAsync(SqlConnection conexion, SqlTransaction transaccion, Prestamo prestamo)
    {
        using var comando = new SqlCommand(
            """
            INSERT INTO Prestamos (ID_Usuario, Fecha_Prestamo, Fecha_Devolucion_Esperada, Estado)
            OUTPUT INSERTED.ID_Prestamo
            VALUES (@IdUsuario, @FechaPrestamo, @FechaDevolucionEsperada, @Estado)
            """, conexion, transaccion);
        comando.Parameters.AddWithValue("@IdUsuario", prestamo.Usuario.IdUsuario);
        comando.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
        comando.Parameters.AddWithValue("@FechaDevolucionEsperada", prestamo.FechaDevolucionEsperada);
        comando.Parameters.AddWithValue("@Estado", prestamo.Estado.ToString());
        return Convert.ToInt32(await comando.ExecuteScalarAsync());
    }

    private static async Task InsertarDetalleYActualizarLibroAsync(SqlConnection conexion, SqlTransaction transaccion, int idPrestamo, DetallePrestamo detalle)
    {
        using (var comandoDetalle = new SqlCommand(
            "INSERT INTO DetallePrestamos (ID_Prestamo, ID_Libro, Cantidad) VALUES (@IdPrestamo, @IdLibro, @Cantidad)",
            conexion, transaccion))
        {
            comandoDetalle.Parameters.AddWithValue("@IdPrestamo", idPrestamo);
            comandoDetalle.Parameters.AddWithValue("@IdLibro", detalle.Libro.IdLibro);
            comandoDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
            await comandoDetalle.ExecuteNonQueryAsync();
        }

        using var comandoLibro = new SqlCommand(
            "UPDATE Libros SET Disponible = Disponible - @Cantidad WHERE ID_Libro = @IdLibro",
            conexion, transaccion);
        comandoLibro.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
        comandoLibro.Parameters.AddWithValue("@IdLibro", detalle.Libro.IdLibro);
        await comandoLibro.ExecuteNonQueryAsync();
    }

    private async Task<List<(int IdPrestamo, int IdUsuario, DateTime FechaPrestamo, DateTime FechaDevolucion, string Estado)>> LeerFilasPrestamoAsync(
        string consultaSql, Action<SqlCommand>? parametros)
    {
        var filas = new List<(int, int, DateTime, DateTime, string)>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(consultaSql, conexion);
        parametros?.Invoke(comando);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            filas.Add((
                lector.GetInt32(lector.GetOrdinal("ID_Prestamo")),
                lector.GetInt32(lector.GetOrdinal("ID_Usuario")),
                lector.GetDateTime(lector.GetOrdinal("Fecha_Prestamo")),
                lector.GetDateTime(lector.GetOrdinal("Fecha_Devolucion_Esperada")),
                lector.GetString(lector.GetOrdinal("Estado"))));
        }
        return filas;
    }

    private async Task<List<Prestamo>> ReconstruirTodosAsync(
        List<(int IdPrestamo, int IdUsuario, DateTime FechaPrestamo, DateTime FechaDevolucion, string Estado)> filas)
    {
        var prestamos = new List<Prestamo>();
        foreach (var fila in filas)
            prestamos.Add(await ReconstruirPrestamoAsync(fila.IdPrestamo, fila.IdUsuario, fila.FechaPrestamo, fila.FechaDevolucion, fila.Estado));
        return prestamos;
    }

    private async Task<Prestamo> ReconstruirPrestamoAsync(int idPrestamo, int idUsuario, DateTime fechaPrestamo, DateTime fechaDevolucion, string estadoTexto)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario)
            ?? throw new InvalidOperationException($"El préstamo {idPrestamo} referencia un usuario inexistente.");

        var estado = Enum.Parse<EstadoPrestamo>(estadoTexto);
        var prestamo = new Prestamo(idPrestamo, usuario, fechaPrestamo, fechaDevolucion, estado);

        foreach (var (idLibro, cantidad) in await ObtenerDetallesAsync(idPrestamo))
        {
            var libro = await _libroRepository.ObtenerPorIdAsync(idLibro)
                ?? throw new InvalidOperationException($"El préstamo {idPrestamo} referencia un libro inexistente.");
            prestamo.CargarDetalleExistente(new DetallePrestamo(libro, cantidad));
        }

        return prestamo;
    }

    private async Task<List<(int IdLibro, int Cantidad)>> ObtenerDetallesAsync(int idPrestamo)
    {
        var detalles = new List<(int, int)>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Libro, Cantidad FROM DetallePrestamos WHERE ID_Prestamo = @IdPrestamo", conexion);
        comando.Parameters.AddWithValue("@IdPrestamo", idPrestamo);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            detalles.Add((lector.GetInt32(0), lector.GetInt32(1)));
        return detalles;
    }
}
