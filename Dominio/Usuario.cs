using System.Text.RegularExpressions;

namespace Biblioteca.Dominio;

public class Usuario
{
    private static readonly Regex FormatoCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public int IdUsuario { get; private set; }
    public string Documento { get; private set; }
    public string Nombre { get; private set; }
    public string Apellidos { get; private set; }
    public string Telefono { get; private set; }
    public string Correo { get; private set; }
    public string ProgramaAcademico { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public Usuario(string documento, string nombre, string apellidos, string telefono, string correo, string programaAcademico)
    {
        if (string.IsNullOrWhiteSpace(documento))
            throw new ArgumentException("El documento es obligatorio.", nameof(documento));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));

        if (!FormatoCorreo.IsMatch(correo))
            throw new ArgumentException("El correo electrónico no tiene un formato válido.", nameof(correo));

        Documento = documento;
        Nombre = nombre;
        Apellidos = apellidos;
        Telefono = telefono;
        Correo = correo;
        ProgramaAcademico = programaAcademico;
    }

    public Usuario(int idUsuario, string documento, string nombre, string apellidos, string telefono, string correo, string programaAcademico)
        : this(documento, nombre, apellidos, telefono, correo, programaAcademico)
    {
        IdUsuario = idUsuario;
    }
}
