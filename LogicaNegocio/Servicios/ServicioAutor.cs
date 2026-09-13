using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.LogicaNegocio.Servicios;

public class ServicioAutor(IAutorRepository autorRepository) : IServicioAutor
{
    private readonly IAutorRepository _autorRepository = autorRepository ?? throw new ArgumentNullException(nameof(autorRepository));

    public Task<IReadOnlyList<Autor>> ObtenerTodosAsync() => _autorRepository.ObtenerTodosAsync();

    public Task<int> RegistrarAsync(Autor autor) => _autorRepository.RegistrarAsync(autor);

    public Task ActualizarAsync(Autor autor) => _autorRepository.ActualizarAsync(autor);

    public async Task EliminarAsync(int idAutor)
    {
        var autor = await _autorRepository.ObtenerPorIdAsync(idAutor)
            ?? throw new EntidadNoEncontradaException($"No existe un autor con el id {idAutor}.");

        await _autorRepository.EliminarAsync(autor.IdAutor);
    }
}
