namespace Biblioteca.LogicaNegocio.Excepciones;

public sealed class RegistroDuplicadoException(string mensaje) : ExcepcionNegocio(mensaje);
