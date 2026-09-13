using Biblioteca.Dominio;

namespace Biblioteca.AccesoDatos.Contratos;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ObtenerTodasAsync();
    Task<Categoria?> ObtenerPorIdAsync(int idCategoria);
    Task<int> RegistrarAsync(Categoria categoria);
    Task ActualizarAsync(Categoria categoria);
    Task EliminarAsync(int idCategoria);
}
