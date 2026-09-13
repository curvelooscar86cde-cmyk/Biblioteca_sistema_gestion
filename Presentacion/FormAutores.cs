using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.Presentacion;

public class FormAutores : Form
{
    private sealed record AutorFila(int IdAutor, string Nombre, string Apellidos, string Nacionalidad, DateTime FechaNacimiento);

    private readonly IServicioAutor _servicioAutor;

    private readonly DataGridView _grilla = new();
    private readonly TextBox _campoNombre = new();
    private readonly TextBox _campoApellidos = new();
    private readonly TextBox _campoNacionalidad = new();
    private readonly DateTimePicker _campoFechaNacimiento = new();
    private readonly Button _botonRegistrar = new();
    private readonly Button _botonActualizar = new();
    private readonly Button _botonEliminar = new();
    private readonly Button _botonLimpiar = new();

    private int? _idAutorSeleccionado;

    public FormAutores(IServicioAutor servicioAutor)
    {
        _servicioAutor = servicioAutor ?? throw new ArgumentNullException(nameof(servicioAutor));
        InicializarComponentes();
        Load += async (_, _) => await CargarAutoresAsync();
    }

    private void InicializarComponentes()
    {
        Text = "Gestión de Autores";
        Width = 700;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        var etiquetaNombre = new Label { Text = "Nombre:", Left = 20, Top = 20, Width = 90 };
        _campoNombre.Left = 120; _campoNombre.Top = 18; _campoNombre.Width = 200;

        var etiquetaApellidos = new Label { Text = "Apellidos:", Left = 20, Top = 55, Width = 90 };
        _campoApellidos.Left = 120; _campoApellidos.Top = 53; _campoApellidos.Width = 200;

        var etiquetaNacionalidad = new Label { Text = "Nacionalidad:", Left = 20, Top = 90, Width = 90 };
        _campoNacionalidad.Left = 120; _campoNacionalidad.Top = 88; _campoNacionalidad.Width = 200;

        var etiquetaFecha = new Label { Text = "Fecha nacimiento:", Left = 20, Top = 125, Width = 110 };
        _campoFechaNacimiento.Left = 140; _campoFechaNacimiento.Top = 123; _campoFechaNacimiento.Width = 180;
        _campoFechaNacimiento.Format = DateTimePickerFormat.Short;

        _botonRegistrar.Text = "Registrar";
        _botonRegistrar.Left = 350; _botonRegistrar.Top = 18; _botonRegistrar.Width = 100;
        _botonRegistrar.Click += async (_, _) => await RegistrarAsync();

        _botonActualizar.Text = "Actualizar";
        _botonActualizar.Left = 460; _botonActualizar.Top = 18; _botonActualizar.Width = 100;
        _botonActualizar.Click += async (_, _) => await ActualizarAsync();

        _botonEliminar.Text = "Eliminar";
        _botonEliminar.Left = 350; _botonEliminar.Top = 53; _botonEliminar.Width = 100;
        _botonEliminar.Click += async (_, _) => await EliminarAsync();

        _botonLimpiar.Text = "Limpiar";
        _botonLimpiar.Left = 460; _botonLimpiar.Top = 53; _botonLimpiar.Width = 100;
        _botonLimpiar.Click += (_, _) => LimpiarFormulario();

        _grilla.Left = 20; _grilla.Top = 170; _grilla.Width = 640; _grilla.Height = 270;
        _grilla.ReadOnly = true;
        _grilla.AllowUserToAddRows = false;
        _grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grilla.MultiSelect = false;
        _grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grilla.SelectionChanged += (_, _) => CargarSeleccionEnFormulario();

        Controls.AddRange([
            etiquetaNombre, _campoNombre,
            etiquetaApellidos, _campoApellidos,
            etiquetaNacionalidad, _campoNacionalidad,
            etiquetaFecha, _campoFechaNacimiento,
            _botonRegistrar, _botonActualizar, _botonEliminar, _botonLimpiar,
            _grilla
        ]);
    }

    private async Task CargarAutoresAsync()
    {
        var autores = await _servicioAutor.ObtenerTodosAsync();
        _grilla.DataSource = autores
            .Select(autor => new AutorFila(autor.IdAutor, autor.Nombre, autor.Apellidos, autor.Nacionalidad, autor.FechaNacimiento))
            .ToList();

        if (_grilla.Columns["IdAutor"] is DataGridViewColumn columnaId)
            columnaId.Visible = false;
    }

    private void CargarSeleccionEnFormulario()
    {
        if (_grilla.CurrentRow?.DataBoundItem is not AutorFila fila)
            return;

        _idAutorSeleccionado = fila.IdAutor;
        _campoNombre.Text = fila.Nombre;
        _campoApellidos.Text = fila.Apellidos;
        _campoNacionalidad.Text = fila.Nacionalidad;
        _campoFechaNacimiento.Value = fila.FechaNacimiento;
    }

    private async Task RegistrarAsync()
    {
        try
        {
            var autor = new Autor(_campoNombre.Text, _campoApellidos.Text, _campoNacionalidad.Text, _campoFechaNacimiento.Value);
            await _servicioAutor.RegistrarAsync(autor);
            MessageBox.Show(this, "Autor registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarAutoresAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ActualizarAsync()
    {
        if (_idAutorSeleccionado is not int idAutor)
        {
            MessageBox.Show(this, "Selecciona un autor de la lista para actualizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var autor = new Autor(idAutor, _campoNombre.Text, _campoApellidos.Text, _campoNacionalidad.Text, _campoFechaNacimiento.Value);
            await _servicioAutor.ActualizarAsync(autor);
            MessageBox.Show(this, "Autor actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarAutoresAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task EliminarAsync()
    {
        if (_idAutorSeleccionado is not int idAutor)
        {
            MessageBox.Show(this, "Selecciona un autor de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(this, "¿Seguro que deseas eliminar este autor?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmacion != DialogResult.Yes)
            return;

        try
        {
            await _servicioAutor.EliminarAsync(idAutor);
            MessageBox.Show(this, "Autor eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarAutoresAsync();
        }
        catch (ExcepcionNegocio ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimpiarFormulario()
    {
        _idAutorSeleccionado = null;
        _campoNombre.Clear();
        _campoApellidos.Clear();
        _campoNacionalidad.Clear();
        _campoFechaNacimiento.Value = DateTime.Today;
    }
}
