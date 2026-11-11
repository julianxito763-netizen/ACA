using SistemaBiblioteca.Negocio;
using SistemaBiblioteca.Datos;
using System.Data;

namespace SistemaBiblioteca.Presentacion;

public class FrmModulo : Form
{
    private readonly ConfiguracionModulo _modulo;
    private readonly DataGridView _grid = new();
    private readonly Dictionary<string, TextBox> _entradas = new();
    private int _idSeleccionado;

    public FrmModulo(ConfiguracionModulo modulo)
    {
        _modulo = modulo; Text = modulo.Titulo; Width = 1050; Height = 680; StartPosition = FormStartPosition.CenterParent;
        var superior = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(31,55,78) };
        superior.Controls.Add(new Label { Text = modulo.Titulo, ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(20, 17) });
        if (modulo.Tabla == "Libros")
        {
            var buscar = new TextBox { Width = 220, Location = new Point(610, 20), PlaceholderText = "Código, título o autor" };
            var btnBuscar = new Button { Text = "Buscar", Location = new Point(840, 18), Width = 80 };
            btnBuscar.Click += (_, _) => Buscar(buscar.Text);
            buscar.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Buscar(buscar.Text); e.SuppressKeyPress = true; } };
            superior.Controls.Add(buscar); superior.Controls.Add(btnBuscar);
        }
        Controls.Add(superior);
        var inferior = new Panel { Dock = DockStyle.Bottom, Height = 240, Padding = new Padding(15) }; Controls.Add(inferior);
        var campos = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 155, AutoScroll = true, WrapContents = true }; inferior.Controls.Add(campos);
        foreach (var campo in modulo.Campos)
        {
            var grupo = new Panel { Width = 245, Height = 72, Margin = new Padding(5) };
            grupo.Controls.Add(new Label { Text = campo.Etiqueta, AutoSize = true, Location = new Point(0, 0) });
            var txt = new TextBox { Width = 225, Location = new Point(0, 25), MaxLength = campo.Nombre == "Correo" ? 120 : 100 };
            grupo.Controls.Add(txt); campos.Controls.Add(grupo); _entradas[campo.Nombre] = txt;
        }
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(0, 8, 0, 0) }; inferior.Controls.Add(acciones);
        Boton("Nuevo / Limpiar", (_, _) => Limpiar()); Boton("Guardar", (_, _) => Guardar()); Boton("Eliminar", (_, _) => Eliminar()); Boton("Actualizar lista", (_, _) => Cargar());
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; _grid.BackgroundColor = Color.White; _grid.SelectionChanged += (_, _) => Seleccionar(); Controls.Add(_grid);
        inferior.BringToFront(); superior.BringToFront();
        Cargar();
        void Boton(string texto, EventHandler accion) { var b = new Button { Text = texto, Width = 145, Height = 34 }; b.Click += accion; acciones.Controls.Add(b); }
    }

    private void Cargar()
    {
        try { _grid.DataSource = ServicioBiblioteca.Listar(_modulo); _grid.ClearSelection(); _idSeleccionado = 0; }
        catch (Exception ex) { MensajeError(ex); }
    }
    private void Buscar(string criterio)
    {
        try { _grid.DataSource = string.IsNullOrWhiteSpace(criterio) ? ServicioBiblioteca.Listar(_modulo) : ServicioBiblioteca.BuscarLibros(criterio); _grid.ClearSelection(); _idSeleccionado = 0; }
        catch (Exception ex) { MensajeError(ex); }
    }
    private void Seleccionar()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
        try
        {
            _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells[_modulo.Clave].Value);
            foreach (var campo in _modulo.Campos)
                if (_grid.Columns.Contains(campo.Nombre)) _entradas[campo.Nombre].Text = Convert.ToString(_grid.CurrentRow.Cells[campo.Nombre].Value) ?? "";
                else if (_modulo.Tabla == "Libros" && campo.Nombre == "AutorId") _entradas[campo.Nombre].Text = Convert.ToString(ObtenerIdRelacionado("Autores", "Autor", _grid.CurrentRow.Cells["Autor"].Value?.ToString())) ?? "";
                else if (_modulo.Tabla == "Libros" && campo.Nombre == "EditorialId") _entradas[campo.Nombre].Text = Convert.ToString(ObtenerIdRelacionado("Editoriales", "Nombre", _grid.CurrentRow.Cells["Editorial"].Value?.ToString())) ?? "";
        }
        catch { _idSeleccionado = 0; }
    }
    private int ObtenerIdRelacionado(string tabla, string columna, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return 0;
        string sql = tabla == "Autores" ? "SELECT TOP 1 AutorId FROM Autores WHERE Nombre+' '+Apellido=@Valor" : "SELECT TOP 1 EditorialId FROM Editoriales WHERE Nombre=@Valor";
        var result = Db.Escalar(sql, new Microsoft.Data.SqlClient.SqlParameter("@Valor", valor));
        return result == null ? 0 : Convert.ToInt32(result);
    }
    private void Guardar()
    {
        try
        {
            var valores = _entradas.ToDictionary(x => x.Key, x => x.Value.Text);
            ServicioBiblioteca.Guardar(_modulo, _idSeleccionado, valores);
            MessageBox.Show("La información se guardó correctamente.", "Biblioteca", MessageBoxButtons.OK, MessageBoxIcon.Information); Limpiar(); Cargar();
        }
        catch (Exception ex) { MensajeError(ex); }
    }
    private void Eliminar()
    {
        if (_idSeleccionado == 0) { MessageBox.Show("Seleccione un registro."); return; }
        if (MessageBox.Show("¿Desea eliminar el registro seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { ServicioBiblioteca.Eliminar(_modulo, _idSeleccionado); Limpiar(); Cargar(); MessageBox.Show("Registro eliminado."); }
        catch (Exception ex) { MensajeError(ex); }
    }
    private void Limpiar() { _idSeleccionado = 0; foreach (var txt in _entradas.Values) txt.Clear(); _grid.ClearSelection(); }
    private static void MensajeError(Exception ex) => MessageBox.Show(ex.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
