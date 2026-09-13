using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Repositorios;

public class AutorRepository(ISqlConnectionFactory conexionFactory) : IAutorRepository
{
    private readonly ISqlConnectionFactory _conexionFactory = conexionFactory ?? throw new ArgumentNullException(nameof(conexionFactory));

    public async Task<IReadOnlyList<Autor>> ObtenerTodosAsync()
    {
        var autores = new List<Autor>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Autor, Nombre, Apellidos, Nacionalidad, Fecha_Nacimiento FROM Autores", conexion);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            autores.Add(MapearAutor(lector));
        return autores;
    }

    public async Task<Autor?> ObtenerPorIdAsync(int idAutor)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Autor, Nombre, Apellidos, Nacionalidad, Fecha_Nacimiento FROM Autores WHERE ID_Autor = @IdAutor", conexion);
        comando.Parameters.AddWithValue("@IdAutor", idAutor);
        using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? MapearAutor(lector) : null;
    }

    public async Task<int> RegistrarAsync(Autor autor)
    {
        ArgumentNullException.ThrowIfNull(autor);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            INSERT INTO Autores (Nombre, Apellidos, Nacionalidad, Fecha_Nacimiento)
            OUTPUT INSERTED.ID_Autor
            VALUES (@Nombre, @Apellidos, @Nacionalidad, @FechaNacimiento)
            """, conexion);
        AgregarParametros(comando, autor);
        var idGenerado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(idGenerado);
    }

    public async Task ActualizarAsync(Autor autor)
    {
        ArgumentNullException.ThrowIfNull(autor);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            UPDATE Autores
            SET Nombre = @Nombre, Apellidos = @Apellidos, Nacionalidad = @Nacionalidad, Fecha_Nacimiento = @FechaNacimiento
            WHERE ID_Autor = @IdAutor
            """, conexion);
        AgregarParametros(comando, autor);
        comando.Parameters.AddWithValue("@IdAutor", autor.IdAutor);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un autor con el id {autor.IdAutor}.");
    }

    public async Task EliminarAsync(int idAutor)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("DELETE FROM Autores WHERE ID_Autor = @IdAutor", conexion);
        comando.Parameters.AddWithValue("@IdAutor", idAutor);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un autor con el id {idAutor}.");
    }

    private static void AgregarParametros(SqlCommand comando, Autor autor)
    {
        comando.Parameters.AddWithValue("@Nombre", autor.Nombre);
        comando.Parameters.AddWithValue("@Apellidos", autor.Apellidos);
        comando.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);
        comando.Parameters.AddWithValue("@FechaNacimiento", autor.FechaNacimiento);
    }

    private static Autor MapearAutor(SqlDataReader lector) =>
        new(
            lector.GetInt32(lector.GetOrdinal("ID_Autor")),
            lector.GetString(lector.GetOrdinal("Nombre")),
            lector.GetString(lector.GetOrdinal("Apellidos")),
            lector.GetString(lector.GetOrdinal("Nacionalidad")),
            lector.GetDateTime(lector.GetOrdinal("Fecha_Nacimiento")));
}
