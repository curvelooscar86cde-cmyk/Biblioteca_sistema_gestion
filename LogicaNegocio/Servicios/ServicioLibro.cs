using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.LogicaNegocio.Servicios;

public class ServicioLibro(ILibroRepository libroRepository) : IServicioLibro
{
    private readonly ILibroRepository _libroRepository = libroRepository ?? throw new ArgumentNullException(nameof(libroRepository));

    public Task<IReadOnlyList<Libro>> ObtenerTodosAsync() => _libroRepository.ObtenerTodosAsync();

    public Task<IReadOnlyList<Libro>> BuscarPorTituloAsync(string titulo) => _libroRepository.BuscarPorTituloAsync(titulo);

    public Task<IReadOnlyList<Libro>> BuscarPorAutorAsync(int idAutor) => _libroRepository.BuscarPorAutorAsync(idAutor);

    public Task<IReadOnlyList<Libro>> BuscarPorCategoriaAsync(int idCategoria) => _libroRepository.BuscarPorCategoriaAsync(idCategoria);

    public async Task<int> RegistrarAsync(Libro libro)
    {
        if (await _libroRepository.ExisteIsbnAsync(libro.Isbn))
            throw new RegistroDuplicadoException($"Ya existe un libro registrado con el ISBN '{libro.Isbn}'.");

        return await _libroRepository.RegistrarAsync(libro);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        var libroExistente = await _libroRepository.ObtenerPorIdAsync(libro.IdLibro)
            ?? throw new EntidadNoEncontradaException($"No existe un libro con el id {libro.IdLibro}.");

        var isbnPerteneceAOtroLibro = libroExistente.Isbn != libro.Isbn && await _libroRepository.ExisteIsbnAsync(libro.Isbn);
        if (isbnPerteneceAOtroLibro)
            throw new RegistroDuplicadoException($"Ya existe otro libro registrado con el ISBN '{libro.Isbn}'.");

        await _libroRepository.ActualizarAsync(libro);
    }

    public async Task EliminarAsync(int idLibro)
    {
        var libro = await _libroRepository.ObtenerPorIdAsync(idLibro)
            ?? throw new EntidadNoEncontradaException($"No existe un libro con el id {idLibro}.");

        await _libroRepository.EliminarAsync(libro.IdLibro);
    }
}
