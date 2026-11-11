# Sistema de Gestión de Biblioteca

Proyecto académico de Programación Avanzada desarrollado con C#, Windows Forms y SQL Server.

## Requisitos
- Windows 10/11 de 64 bits.
- Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET**.
- .NET 8 SDK.
- SQL Server Express (instancia predeterminada `SQLEXPRESS`).
- SQL Server Management Studio (SSMS).
- Acceso a Internet la primera vez para restaurar `Microsoft.Data.SqlClient` desde NuGet.

## Instalación
1. Instale SQL Server Express y confirme que el servicio de la instancia `SQLEXPRESS` esté iniciado.
2. Abra SSMS y conéctese a `localhost\SQLEXPRESS` usando autenticación de Windows.
3. Abra `BaseDatos/Biblioteca.sql` y ejecute el script completo. El script crea la base de datos `BibliotecaDB`, sus tablas y datos de ejemplo. No elimina las tablas existentes.
4. Abra `SistemaBiblioteca/SistemaBiblioteca.csproj` en Visual Studio 2022.
5. Espere a que NuGet restaure `Microsoft.Data.SqlClient`.
6. Ejecute con F5.

## Configuración de conexión
La cadena se encuentra en `SistemaBiblioteca/Datos/Db.cs`:

```csharp
Server=.\SQLEXPRESS;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Si su instancia tiene otro nombre, modifique `Server`. Para una instalación SQL Server LocalDB, use por ejemplo `Server=(localdb)\MSSQLLocalDB;...` si tiene esa instancia instalada.

## Funcionalidades incluidas
- CRUD de autores, editoriales, usuarios y libros.
- Consulta de registros en tablas.
- Registro de préstamos mediante selección de usuario y libro, con validación de fechas y existencias.
- Devolución de préstamos y actualización de existencias.
- Reportes de libros, usuarios, préstamos activos e historial.
- Consultas parametrizadas y manejo de errores.

## Consideraciones
- En el formulario de libros se ingresan los identificadores numéricos de autor y editorial. Se pueden consultar en sus respectivos módulos.
- No se permite eliminar registros relacionados cuando SQL Server detecta una referencia mediante clave foránea.
- El script incluye registros de demostración. Puede utilizarlos para hacer pruebas locales.
- Antes de entregar, compile y ejecute el proyecto en el equipo de destino, realice las pruebas descritas en `Documentacion/DocumentoTecnico.md` y agregue capturas reales en `Evidencias/`.

## Estructura
- `SistemaBiblioteca/`: aplicación Windows Forms.
- `BaseDatos/Biblioteca.sql`: creación de base de datos y datos iniciales.
- `Documentacion/DocumentoTecnico.md`: documento técnico del proyecto.
- `Evidencias/`: espacio para capturas tomadas durante las pruebas reales.
