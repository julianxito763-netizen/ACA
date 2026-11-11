using Microsoft.Data.SqlClient;
using System.Data;

namespace SistemaBiblioteca.Datos;

public static class Db
{
    // Cambie el servidor si su instancia de SQL Server tiene otro nombre.
    private const string CadenaConexion = @"Server=.\SQLEXPRESS;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection CrearConexion() => new(CadenaConexion);

    public static void ProbarConexion()
    {
        using var conexion = CrearConexion();
        conexion.Open();
    }

    public static DataTable Consultar(string sql, params SqlParameter[] parametros)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(sql, conexion);
        if (parametros.Length > 0) comando.Parameters.AddRange(parametros);
        using var adaptador = new SqlDataAdapter(comando);
        var tabla = new DataTable();
        adaptador.Fill(tabla);
        return tabla;
    }

    public static int Ejecutar(string sql, params SqlParameter[] parametros)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        using var comando = new SqlCommand(sql, conexion);
        if (parametros.Length > 0) comando.Parameters.AddRange(parametros);
        return comando.ExecuteNonQuery();
    }

    public static object? Escalar(string sql, params SqlParameter[] parametros)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        using var comando = new SqlCommand(sql, conexion);
        if (parametros.Length > 0) comando.Parameters.AddRange(parametros);
        return comando.ExecuteScalar();
    }
}
