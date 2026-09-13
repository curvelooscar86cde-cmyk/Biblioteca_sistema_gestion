using Biblioteca.Dominio;

namespace Biblioteca.AccesoDatos.Contratos;

public interface IAutorRepository
{
    Task<IReadOnlyList<Autor>> ObtenerTodosAsync();
    Task<Autor?> ObtenerPorIdAsync(int idAutor);
    Task<int> RegistrarAsync(Autor autor);
    Task ActualizarAsync(Autor autor);
    Task EliminarAsync(int idAutor);
}
