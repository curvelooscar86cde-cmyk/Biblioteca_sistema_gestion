namespace Biblioteca.LogicaNegocio.Excepciones;

public sealed class EntidadNoEncontradaException(string mensaje) : ExcepcionNegocio(mensaje);
