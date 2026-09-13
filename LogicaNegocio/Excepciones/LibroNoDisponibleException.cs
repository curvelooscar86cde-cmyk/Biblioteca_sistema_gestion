namespace Biblioteca.LogicaNegocio.Excepciones;

public sealed class LibroNoDisponibleException(string mensaje) : ExcepcionNegocio(mensaje);
