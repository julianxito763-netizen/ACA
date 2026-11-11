using Microsoft.Data.SqlClient;
using SistemaBiblioteca.Datos;
using System.Data;
using System.Text.RegularExpressions;

namespace SistemaBiblioteca.Negocio;

public static class ServicioBiblioteca
{
    public static DataTable Listar(ConfiguracionModulo m)
    {
        string sql = m.Tabla == "Libros"
            ? $"SELECT {m.ColumnasConsulta} FROM Libros l INNER JOIN Autores a ON l.AutorId=a.AutorId INNER JOIN Editoriales e ON l.EditorialId=e.EditorialId ORDER BY {m.Orden}"
            : $"SELECT {m.ColumnasConsulta} FROM {m.Tabla} ORDER BY {m.Orden}";
        return Db.Consultar(sql);
    }

    public static void Guardar(ConfiguracionModulo m, int id, Dictionary<string,string> valores)
    {
        foreach (var campo in m.Campos)
        {
            string valor = valores[campo.Nombre].Trim();
            if (string.IsNullOrWhiteSpace(valor)) throw new ArgumentException($"El campo {campo.Etiqueta} es obligatorio.");
            if (campo.Numerico && !int.TryParse(valor, out _)) throw new ArgumentException($"El campo {campo.Etiqueta} debe ser un número entero.");
        }
        if (m.Tabla == "Usuarios")
        {
            string correo = valores["Correo"].Trim();
            if (correo.Length > 0 && !Regex.IsMatch(correo, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                throw new ArgumentException("Ingrese un correo electrónico válido.");
        }
        if (m.Tabla == "Libros")
        {
            int anio = int.Parse(valores["AnioPublicacion"]);
            int existencias = int.Parse(valores["Existencias"]);
            if (anio < 0 || anio > DateTime.Now.Year + 1) throw new ArgumentException("El año de publicación no es válido.");
            if (existencias < 0) throw new ArgumentException("Las existencias no pueden ser negativas.");
        }
        var parametros = new List<SqlParameter>();
        foreach (var c in m.Campos)
        {
            object valor = c.Numerico ? int.Parse(valores[c.Nombre]) : valores[c.Nombre].Trim();
            parametros.Add(new SqlParameter("@" + c.Nombre, valor));
        }
        if (id == 0)
        {
            string columnas = string.Join(",", m.Campos.Select(c => c.Nombre));
            string parametrosSql = string.Join(",", m.Campos.Select(c => "@" + c.Nombre));
            Db.Ejecutar($"INSERT INTO {m.Tabla} ({columnas}) VALUES ({parametrosSql})", parametros.ToArray());
        }
        else
        {
            parametros.Add(new SqlParameter("@Id", id));
            string set = string.Join(",", m.Campos.Select(c => c.Nombre + "=@" + c.Nombre));
            Db.Ejecutar($"UPDATE {m.Tabla} SET {set} WHERE {m.Clave}=@Id", parametros.ToArray());
        }
    }

    public static DataTable BuscarLibros(string criterio) => Db.Consultar(@"
        SELECT l.LibroId, l.Codigo, l.Titulo, a.Nombre + ' ' + a.Apellido AS Autor,
               e.Nombre AS Editorial, l.AnioPublicacion, l.Existencias
        FROM Libros l INNER JOIN Autores a ON l.AutorId=a.AutorId
        INNER JOIN Editoriales e ON l.EditorialId=e.EditorialId
        WHERE l.Codigo LIKE @Criterio OR l.Titulo LIKE @Criterio
           OR a.Nombre LIKE @Criterio OR a.Apellido LIKE @Criterio
           OR a.Nombre + ' ' + a.Apellido LIKE @Criterio
        ORDER BY l.Titulo", new SqlParameter("@Criterio", "%" + criterio.Trim() + "%"));

    public static void Eliminar(ConfiguracionModulo m, int id)
    {
        if (id <= 0) throw new ArgumentException("Seleccione un registro para eliminar.");
        Db.Ejecutar($"DELETE FROM {m.Tabla} WHERE {m.Clave}=@Id", new SqlParameter("@Id", id));
    }

    public static DataTable ListarPrestamos() => Db.Consultar(@"
        SELECT p.PrestamoId, u.Nombre + ' ' + u.Apellido AS Usuario,
               l.Titulo AS Libro, p.FechaPrestamo, p.FechaLimite,
               p.FechaDevolucion, p.Estado
        FROM Prestamos p INNER JOIN Usuarios u ON p.UsuarioId=u.UsuarioId
        INNER JOIN Libros l ON p.LibroId=l.LibroId ORDER BY p.PrestamoId DESC");

    public static void CrearPrestamo(int usuarioId, int libroId, DateTime fecha, DateTime limite)
    {
        if (usuarioId <= 0 || libroId <= 0) throw new ArgumentException("Seleccione un usuario y un libro válidos.");
        if (limite.Date < fecha.Date) throw new ArgumentException("La fecha límite no puede ser anterior al préstamo.");
        using var cn = Db.CrearConexion(); cn.Open(); using var tx = cn.BeginTransaction();
        try
        {
            using var verificar = new SqlCommand("SELECT Existencias FROM Libros WITH (UPDLOCK, ROWLOCK) WHERE LibroId=@LibroId", cn, tx);
            verificar.Parameters.AddWithValue("@LibroId", libroId);
            object? stock = verificar.ExecuteScalar();
            if (stock == null) throw new InvalidOperationException("El libro seleccionado no existe.");
            if (Convert.ToInt32(stock) < 1) throw new InvalidOperationException("No hay existencias disponibles de este libro.");
            using var usuario = new SqlCommand("SELECT COUNT(1) FROM Usuarios WHERE UsuarioId=@UsuarioId", cn, tx);
            usuario.Parameters.AddWithValue("@UsuarioId", usuarioId);
            if (Convert.ToInt32(usuario.ExecuteScalar()) == 0) throw new InvalidOperationException("El usuario seleccionado no existe.");
            using var insertar = new SqlCommand("INSERT INTO Prestamos(UsuarioId,LibroId,FechaPrestamo,FechaLimite,Estado) VALUES(@UsuarioId,@LibroId,@Fecha,@Limite,'Prestado')", cn, tx);
            insertar.Parameters.AddWithValue("@UsuarioId", usuarioId); insertar.Parameters.AddWithValue("@LibroId", libroId);
            insertar.Parameters.AddWithValue("@Fecha", fecha.Date); insertar.Parameters.AddWithValue("@Limite", limite.Date); insertar.ExecuteNonQuery();
            using var actualizar = new SqlCommand("UPDATE Libros SET Existencias=Existencias-1 WHERE LibroId=@LibroId", cn, tx);
            actualizar.Parameters.AddWithValue("@LibroId", libroId); actualizar.ExecuteNonQuery(); tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public static void EditarPrestamo(int prestamoId, DateTime fecha, DateTime limite)
    {
        if (prestamoId <= 0) throw new ArgumentException("Seleccione un préstamo.");
        if (limite.Date < fecha.Date) throw new ArgumentException("La fecha límite no puede ser anterior al préstamo.");
        int filas = Db.Ejecutar("UPDATE Prestamos SET FechaPrestamo=@Fecha, FechaLimite=@Limite WHERE PrestamoId=@Id AND Estado='Prestado'",
            new SqlParameter("@Fecha", fecha.Date), new SqlParameter("@Limite", limite.Date), new SqlParameter("@Id", prestamoId));
        if (filas == 0) throw new InvalidOperationException("No se encontró un préstamo activo para modificar.");
    }

    public static void EliminarPrestamo(int prestamoId)
    {
        if (prestamoId <= 0) throw new ArgumentException("Seleccione un préstamo.");
        using var cn = Db.CrearConexion(); cn.Open(); using var tx = cn.BeginTransaction();
        try
        {
            int? libroId = null; string estado = "";
            using (var buscar = new SqlCommand("SELECT LibroId,Estado FROM Prestamos WITH (UPDLOCK, ROWLOCK) WHERE PrestamoId=@Id", cn, tx))
            {
                buscar.Parameters.AddWithValue("@Id", prestamoId);
                using var lector = buscar.ExecuteReader();
                if (!lector.Read()) throw new InvalidOperationException("No se encontró el préstamo.");
                libroId = lector.GetInt32(0); estado = lector.GetString(1);
            }
            using (var borrar = new SqlCommand("DELETE FROM Prestamos WHERE PrestamoId=@Id", cn, tx))
            { borrar.Parameters.AddWithValue("@Id", prestamoId); borrar.ExecuteNonQuery(); }
            if (estado == "Prestado")
            {
                using var stock = new SqlCommand("UPDATE Libros SET Existencias=Existencias+1 WHERE LibroId=@LibroId", cn, tx);
                stock.Parameters.AddWithValue("@LibroId", libroId.Value); stock.ExecuteNonQuery();
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public static void RegistrarDevolucion(int prestamoId)
    {
        if (prestamoId <= 0) throw new ArgumentException("Seleccione un préstamo.");
        using var cn = Db.CrearConexion(); cn.Open(); using var tx = cn.BeginTransaction();
        try
        {
            int libroId;
            using (var buscar = new SqlCommand("SELECT LibroId FROM Prestamos WITH (UPDLOCK, ROWLOCK) WHERE PrestamoId=@Id AND Estado='Prestado'", cn, tx))
            {
                buscar.Parameters.AddWithValue("@Id", prestamoId); object? resultado = buscar.ExecuteScalar();
                if (resultado == null) throw new InvalidOperationException("El préstamo no existe o ya fue devuelto.");
                libroId = Convert.ToInt32(resultado);
            }
            using (var devolver = new SqlCommand("UPDATE Prestamos SET FechaDevolucion=GETDATE(), Estado='Devuelto' WHERE PrestamoId=@Id", cn, tx))
            { devolver.Parameters.AddWithValue("@Id", prestamoId); devolver.ExecuteNonQuery(); }
            using (var stock = new SqlCommand("UPDATE Libros SET Existencias=Existencias+1 WHERE LibroId=@LibroId", cn, tx))
            { stock.Parameters.AddWithValue("@LibroId", libroId); stock.ExecuteNonQuery(); }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public static DataTable Reporte(string tipo) => tipo switch
    {
        "Libros" => Db.Consultar("SELECT l.Codigo,l.Titulo,a.Nombre+' '+a.Apellido AS Autor,e.Nombre AS Editorial,l.AnioPublicacion,l.Existencias FROM Libros l INNER JOIN Autores a ON l.AutorId=a.AutorId INNER JOIN Editoriales e ON l.EditorialId=e.EditorialId ORDER BY l.Titulo"),
        "Usuarios" => Db.Consultar("SELECT Nombre,Apellido,Documento,Telefono,Correo FROM Usuarios ORDER BY Apellido,Nombre"),
        "Préstamos activos" => Db.Consultar("SELECT p.PrestamoId,u.Nombre+' '+u.Apellido AS Usuario,l.Titulo,p.FechaPrestamo,p.FechaLimite,p.Estado FROM Prestamos p INNER JOIN Usuarios u ON p.UsuarioId=u.UsuarioId INNER JOIN Libros l ON p.LibroId=l.LibroId WHERE p.Estado='Prestado' ORDER BY p.FechaLimite"),
        _ => Db.Consultar("SELECT p.PrestamoId,u.Nombre+' '+u.Apellido AS Usuario,l.Titulo,p.FechaPrestamo,p.FechaLimite,p.FechaDevolucion,p.Estado FROM Prestamos p INNER JOIN Usuarios u ON p.UsuarioId=u.UsuarioId INNER JOIN Libros l ON p.LibroId=l.LibroId ORDER BY p.FechaPrestamo DESC")
    };
}
