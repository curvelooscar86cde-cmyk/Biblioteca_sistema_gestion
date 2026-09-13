using Biblioteca.Dominio;

namespace Biblioteca.LogicaNegocio.Contratos;

public interface IServicioLibro
{
    Task<IReadOnlyList<Libro>> ObtenerTodosAsync();
    Task<IReadOnlyList<Libro>> BuscarPorTituloAsync(string titulo);
    Task<IReadOnlyList<Libro>> BuscarPorAutorAsync(int idAutor);
    Task<IReadOnlyList<Libro>> BuscarPorCategoriaAsync(int idCategoria);
    Task<int> RegistrarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int idLibro);
}
