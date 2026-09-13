using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.Presentacion;

public class FormCategorias : Form
{
    private sealed record CategoriaFila(int IdCategoria, string Nombre);

    private readonly IServicioCategoria _servicioCategoria;

    private readonly DataGridView _grilla = new();
    private readonly TextBox _campoNombre = new();
    private readonly Button _botonRegistrar = new();
    private readonly Button _botonActualizar = new();
    private readonly Button _botonEliminar = new();
    private readonly Button _botonLimpiar = new();

    private int? _idCategoriaSeleccionada;

    public FormCategorias(IServicioCategoria servicioCategoria)
    {
        _servicioCategoria = servicioCategoria ?? throw new ArgumentNullException(nameof(servicioCategoria));
        InicializarComponentes();
        Load += async (_, _) => await CargarCategoriasAsync();
    }

    private void InicializarComponentes()
    {
        Text = "Gestión de Categorías";
        Width = 500;
        Height = 450;
        StartPosition = FormStartPosition.CenterParent;

        var etiquetaNombre = new Label { Text = "Nombre:", Left = 20, Top = 20, Width = 80 };
        _campoNombre.Left = 110; _campoNombre.Top = 18; _campoNombre.Width = 200;

        _botonRegistrar.Text = "Registrar";
        _botonRegistrar.Left = 330; _botonRegistrar.Top = 16; _botonRegistrar.Width = 130;
        _botonRegistrar.Click += async (_, _) => await RegistrarAsync();

        _botonActualizar.Text = "Actualizar";
        _botonActualizar.Left = 20; _botonActualizar.Top = 55; _botonActualizar.Width = 130;
        _botonActualizar.Click += async (_, _) => await ActualizarAsync();

        _botonEliminar.Text = "Eliminar";
        _botonEliminar.Left = 160; _botonEliminar.Top = 55; _botonEliminar.Width = 130;
        _botonEliminar.Click += async (_, _) => await EliminarAsync();

        _botonLimpiar.Text = "Limpiar";
        _botonLimpiar.Left = 300; _botonLimpiar.Top = 55; _botonLimpiar.Width = 130;
        _botonLimpiar.Click += (_, _) => LimpiarFormulario();

        _grilla.Left = 20; _grilla.Top = 100; _grilla.Width = 440; _grilla.Height = 300;
        _grilla.ReadOnly = true;
        _grilla.AllowUserToAddRows = false;
        _grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grilla.MultiSelect = false;
        _grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grilla.SelectionChanged += (_, _) => CargarSeleccionEnFormulario();

        Controls.AddRange([etiquetaNombre, _campoNombre, _botonRegistrar, _botonActualizar, _botonEliminar, _botonLimpiar, _grilla]);
    }

    private async Task CargarCategoriasAsync()
    {
        var categorias = await _servicioCategoria.ObtenerTodasAsync();
        _grilla.DataSource = categorias
            .Select(categoria => new CategoriaFila(categoria.IdCategoria, categoria.Nombre))
            .ToList();

        if (_grilla.Columns["IdCategoria"] is DataGridViewColumn columnaId)
            columnaId.Visible = false;
    }

    private void CargarSeleccionEnFormulario()
    {
        if (_grilla.CurrentRow?.DataBoundItem is not CategoriaFila fila)
            return;

        _idCategoriaSeleccionada = fila.IdCategoria;
        _campoNombre.Text = fila.Nombre;
    }

    private async Task RegistrarAsync()
    {
        try
        {
            var categoria = new Categoria(_campoNombre.Text);
            await _servicioCategoria.RegistrarAsync(categoria);
            MessageBox.Show(this, "Categoría registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarCategoriasAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ActualizarAsync()
    {
        if (_idCategoriaSeleccionada is not int idCategoria)
        {
            MessageBox.Show(this, "Selecciona una categoría de la lista para actualizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var categoria = new Categoria(idCategoria, _campoNombre.Text);
            await _servicioCategoria.ActualizarAsync(categoria);
            MessageBox.Show(this, "Categoría actualizada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarCategoriasAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task EliminarAsync()
    {
        if (_idCategoriaSeleccionada is not int idCategoria)
        {
            MessageBox.Show(this, "Selecciona una categoría de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(this, "¿Seguro que deseas eliminar esta categoría?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmacion != DialogResult.Yes)
            return;

        try
        {
            await _servicioCategoria.EliminarAsync(idCategoria);
            MessageBox.Show(this, "Categoría eliminada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarCategoriasAsync();
        }
        catch (ExcepcionNegocio ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimpiarFormulario()
    {
        _idCategoriaSeleccionada = null;
        _campoNombre.Clear();
    }
}
