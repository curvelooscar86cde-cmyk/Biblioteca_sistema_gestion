using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Repositorios;

public class LibroRepository(ISqlConnectionFactory conexionFactory) : ILibroRepository
{
    private const string ConsultaBase =
        """
        SELECT l.ID_Libro, l.Codigo, l.Titulo, l.ISBN, l.Cantidad, l.Disponible,
               a.ID_Autor, a.Nombre AS NombreAutor, a.Apellidos AS ApellidosAutor, a.Nacionalidad, a.Fecha_Nacimiento,
               c.ID_Categoria, c.Nombre AS NombreCategoria
        FROM Libros l
        INNER JOIN Autores a ON l.ID_Autor = a.ID_Autor
        INNER JOIN Categorias c ON l.ID_Categoria = c.ID_Categoria
        """;

    private readonly ISqlConnectionFactory _conexionFactory = conexionFactory ?? throw new ArgumentNullException(nameof(conexionFactory));

    public async Task<IReadOnlyList<Libro>> ObtenerTodosAsync() =>
        await EjecutarConsultaAsync(ConsultaBase, parametros: null);

    public async Task<Libro?> ObtenerPorIdAsync(int idLibro)
    {
        var resultado = await EjecutarConsultaAsync(
            $"{ConsultaBase} WHERE l.ID_Libro = @IdLibro",
            comando => comando.Parameters.AddWithValue("@IdLibro", idLibro));
        return resultado.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Libro>> BuscarPorTituloAsync(string titulo) =>
        await EjecutarConsultaAsync(
            $"{ConsultaBase} WHERE l.Titulo LIKE @Titulo",
            comando => comando.Parameters.AddWithValue("@Titulo", $"%{titulo}%"));

    public async Task<IReadOnlyList<Libro>> BuscarPorAutorAsync(int idAutor) =>
        await EjecutarConsultaAsync(
            $"{ConsultaBase} WHERE a.ID_Autor = @IdAutor",
            comando => comando.Parameters.AddWithValue("@IdAutor", idAutor));

    public async Task<IReadOnlyList<Libro>> BuscarPorCategoriaAsync(int idCategoria) =>
        await EjecutarConsultaAsync(
            $"{ConsultaBase} WHERE c.ID_Categoria = @IdCategoria",
            comando => comando.Parameters.AddWithValue("@IdCategoria", idCategoria));

    public async Task<bool> ExisteIsbnAsync(string isbn)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("SELECT COUNT(1) FROM Libros WHERE ISBN = @Isbn", conexion);
        comando.Parameters.AddWithValue("@Isbn", isbn);
        var cantidad = Convert.ToInt32(await comando.ExecuteScalarAsync());
        return cantidad > 0;
    }

    public async Task<int> RegistrarAsync(Libro libro)
    {
        ArgumentNullException.ThrowIfNull(libro);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            INSERT INTO Libros (Codigo, Titulo, ISBN, Cantidad, Disponible, ID_Autor, ID_Categoria)
            OUTPUT INSERTED.ID_Libro
            VALUES (@Codigo, @Titulo, @Isbn, @Cantidad, @Disponible, @IdAutor, @IdCategoria)
            """, conexion);
        AgregarParametros(comando, libro);
        var idGenerado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(idGenerado);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        ArgumentNullException.ThrowIfNull(libro);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            UPDATE Libros
            SET Codigo = @Codigo, Titulo = @Titulo, ISBN = @Isbn, Cantidad = @Cantidad,
                Disponible = @Disponible, ID_Autor = @IdAutor, ID_Categoria = @IdCategoria
            WHERE ID_Libro = @IdLibro
            """, conexion);
        AgregarParametros(comando, libro);
        comando.Parameters.AddWithValue("@IdLibro", libro.IdLibro);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un libro con el id {libro.IdLibro}.");
    }

    public async Task EliminarAsync(int idLibro)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("DELETE FROM Libros WHERE ID_Libro = @IdLibro", conexion);
        comando.Parameters.AddWithValue("@IdLibro", idLibro);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un libro con el id {idLibro}.");
    }

    private async Task<IReadOnlyList<Libro>> EjecutarConsultaAsync(string consultaSql, Action<SqlCommand>? parametros)
    {
        var libros = new List<Libro>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(consultaSql, conexion);
        parametros?.Invoke(comando);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            libros.Add(MapearLibro(lector));
        return libros;
    }

    private static void AgregarParametros(SqlCommand comando, Libro libro)
    {
        comando.Parameters.AddWithValue("@Codigo", libro.Codigo);
        comando.Parameters.AddWithValue("@Titulo", libro.Titulo);
        comando.Parameters.AddWithValue("@Isbn", libro.Isbn);
        comando.Parameters.AddWithValue("@Cantidad", libro.Cantidad);
        comando.Parameters.AddWithValue("@Disponible", libro.Disponible);
        comando.Parameters.AddWithValue("@IdAutor", libro.Autor.IdAutor);
        comando.Parameters.AddWithValue("@IdCategoria", libro.Categoria.IdCategoria);
    }

    private static Libro MapearLibro(SqlDataReader lector)
    {
        var autor = new Autor(
            lector.GetInt32(lector.GetOrdinal("ID_Autor")),
            lector.GetString(lector.GetOrdinal("NombreAutor")),
            lector.GetString(lector.GetOrdinal("ApellidosAutor")),
            lector.GetString(lector.GetOrdinal("Nacionalidad")),
            lector.GetDateTime(lector.GetOrdinal("Fecha_Nacimiento")));

        var categoria = new Categoria(
            lector.GetInt32(lector.GetOrdinal("ID_Categoria")),
            lector.GetString(lector.GetOrdinal("NombreCategoria")));

        return new Libro(
            lector.GetInt32(lector.GetOrdinal("ID_Libro")),
            lector.GetString(lector.GetOrdinal("Codigo")),
            lector.GetString(lector.GetOrdinal("Titulo")),
            lector.GetString(lector.GetOrdinal("ISBN")),
            lector.GetInt32(lector.GetOrdinal("Cantidad")),
            lector.GetInt32(lector.GetOrdinal("Disponible")),
            autor,
            categoria);
    }
}
