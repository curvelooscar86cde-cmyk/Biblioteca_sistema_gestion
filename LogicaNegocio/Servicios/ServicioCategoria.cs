using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.LogicaNegocio.Servicios;

public class ServicioCategoria(ICategoriaRepository categoriaRepository) : IServicioCategoria
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));

    public Task<IReadOnlyList<Categoria>> ObtenerTodasAsync() => _categoriaRepository.ObtenerTodasAsync();

    public async Task<int> RegistrarAsync(Categoria categoria)
    {
        var categorias = await _categoriaRepository.ObtenerTodasAsync();
        var yaExiste = categorias.Any(c => string.Equals(c.Nombre, categoria.Nombre, StringComparison.OrdinalIgnoreCase));

        if (yaExiste)
            throw new RegistroDuplicadoException($"Ya existe una categoría llamada '{categoria.Nombre}'.");

        return await _categoriaRepository.RegistrarAsync(categoria);
    }

    public Task ActualizarAsync(Categoria categoria) => _categoriaRepository.ActualizarAsync(categoria);

    public async Task EliminarAsync(int idCategoria)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(idCategoria)
            ?? throw new EntidadNoEncontradaException($"No existe una categoría con el id {idCategoria}.");

        await _categoriaRepository.EliminarAsync(categoria.IdCategoria);
    }
}
