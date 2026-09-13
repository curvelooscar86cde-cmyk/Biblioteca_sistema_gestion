using Biblioteca.Dominio;

namespace Biblioteca.AccesoDatos.Contratos;

public interface ILibroRepository
{
    Task<IReadOnlyList<Libro>> ObtenerTodosAsync();
    Task<Libro?> ObtenerPorIdAsync(int idLibro);
    Task<IReadOnlyList<Libro>> BuscarPorTituloAsync(string titulo);
    Task<IReadOnlyList<Libro>> BuscarPorAutorAsync(int idAutor);
    Task<IReadOnlyList<Libro>> BuscarPorCategoriaAsync(int idCategoria);
    Task<bool> ExisteIsbnAsync(string isbn);
    Task<int> RegistrarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int idLibro);
}
