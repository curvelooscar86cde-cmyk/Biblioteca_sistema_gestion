using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.AccesoDatos.Repositorios;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Servicios;
using Biblioteca.Presentacion;

namespace Biblioteca;

internal static class Program
{
    // Reemplaza "OSCARC\\SQLEXPRESS" por el nombre real de tu instancia de SQL Server.
    private const string CadenaConexion =
        "Server=OSCARC\\SQLEXPRESS;Database=Biblioteca;Trusted_Connection=True;TrustServerCertificate=True;";

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        ISqlConnectionFactory conexionFactory = new SqlConnectionFactory(CadenaConexion);

        IAutorRepository autorRepository = new AutorRepository(conexionFactory);
        ICategoriaRepository categoriaRepository = new CategoriaRepository(conexionFactory);
        ILibroRepository libroRepository = new LibroRepository(conexionFactory);
        IUsuarioRepository usuarioRepository = new UsuarioRepository(conexionFactory);
        IPrestamoRepository prestamoRepository = new PrestamoRepository(conexionFactory, usuarioRepository, libroRepository);

        IServicioAutor servicioAutor = new ServicioAutor(autorRepository);
        IServicioCategoria servicioCategoria = new ServicioCategoria(categoriaRepository);
        IServicioLibro servicioLibro = new ServicioLibro(libroRepository);
        IServicioUsuario servicioUsuario = new ServicioUsuario(usuarioRepository);
        IServicioPrestamo servicioPrestamo = new ServicioPrestamo(prestamoRepository, usuarioRepository, libroRepository);

        Application.Run(new FormPrincipal(servicioAutor, servicioCategoria, servicioLibro, servicioUsuario, servicioPrestamo));
    }
}
