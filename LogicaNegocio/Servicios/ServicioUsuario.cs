using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.LogicaNegocio.Servicios;

public class ServicioUsuario(IUsuarioRepository usuarioRepository) : IServicioUsuario
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));

    public Task<IReadOnlyList<Usuario>> ObtenerTodosAsync() => _usuarioRepository.ObtenerTodosAsync();

    public async Task<int> RegistrarAsync(Usuario usuario)
    {
        if (await _usuarioRepository.ExisteDocumentoAsync(usuario.Documento))
            throw new RegistroDuplicadoException($"Ya existe un usuario registrado con el documento '{usuario.Documento}'.");

        return await _usuarioRepository.RegistrarAsync(usuario);
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        var usuarioExistente = await _usuarioRepository.ObtenerPorIdAsync(usuario.IdUsuario)
            ?? throw new EntidadNoEncontradaException($"No existe un usuario con el id {usuario.IdUsuario}.");

        var documentoPerteneceAOtroUsuario = usuarioExistente.Documento != usuario.Documento
            && await _usuarioRepository.ExisteDocumentoAsync(usuario.Documento);

        if (documentoPerteneceAOtroUsuario)
            throw new RegistroDuplicadoException($"Ya existe otro usuario registrado con el documento '{usuario.Documento}'.");

        await _usuarioRepository.ActualizarAsync(usuario);
    }

    public async Task EliminarAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario)
            ?? throw new EntidadNoEncontradaException($"No existe un usuario con el id {idUsuario}.");

        await _usuarioRepository.EliminarAsync(usuario.IdUsuario);
    }
}
