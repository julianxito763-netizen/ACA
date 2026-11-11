using SistemaBiblioteca.Negocio;

namespace SistemaBiblioteca.Presentacion;

public class FrmReportes : Form
{
    private readonly DataGridView _grid = new();
    private readonly ComboBox _tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
    public FrmReportes()
    {
        Text = "Reportes"; Width = 1000; Height = 620; StartPosition = FormStartPosition.CenterParent;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(15) }; Controls.Add(panel);
        panel.Controls.Add(new Label { Text = "Tipo de reporte:", AutoSize = true, Margin = new Padding(5, 10, 5, 0) }); _tipo.Items.AddRange(["Libros", "Usuarios", "Préstamos activos", "Historial de préstamos"]); _tipo.SelectedIndex = 0; panel.Controls.Add(_tipo);
        var ver = new Button { Text = "Consultar", Width = 110, Height = 30 }; ver.Click += (_, _) => Consultar(); panel.Controls.Add(ver);
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(_grid); panel.BringToFront(); Consultar();
    }
    private void Consultar() { try { _grid.DataSource = ServicioBiblioteca.Reporte(_tipo.Text); } catch (Exception ex) { MessageBox.Show(ex.Message, "Error al generar el reporte"); } }
}
