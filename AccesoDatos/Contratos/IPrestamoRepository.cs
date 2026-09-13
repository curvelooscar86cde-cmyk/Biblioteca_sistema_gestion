using Biblioteca.Dominio;

namespace Biblioteca.AccesoDatos.Contratos;

public interface IPrestamoRepository
{
    Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync();
    Task<IReadOnlyList<Prestamo>> ObtenerPorUsuarioAsync(int idUsuario);
    Task<Prestamo?> ObtenerPorIdAsync(int idPrestamo);
    Task<int> RegistrarAsync(Prestamo prestamo);
    Task ActualizarEstadoAsync(Prestamo prestamo);
}
