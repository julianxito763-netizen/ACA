/* Sistema de Gestión de Biblioteca - SQL Server */
IF DB_ID('BibliotecaDB') IS NULL CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;
GO
IF OBJECT_ID('dbo.Autores','U') IS NULL
CREATE TABLE Autores(
 AutorId INT IDENTITY(1,1) PRIMARY KEY,
 Nombre NVARCHAR(80) NOT NULL,
 Apellido NVARCHAR(80) NOT NULL
);
IF OBJECT_ID('dbo.Editoriales','U') IS NULL
CREATE TABLE Editoriales(
 EditorialId INT IDENTITY(1,1) PRIMARY KEY,
 Nombre NVARCHAR(120) NOT NULL UNIQUE,
 Ciudad NVARCHAR(80) NULL
);
IF OBJECT_ID('dbo.Usuarios','U') IS NULL
CREATE TABLE Usuarios(
 UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
 Nombre NVARCHAR(80) NOT NULL,
 Apellido NVARCHAR(80) NOT NULL,
 Documento NVARCHAR(25) NOT NULL UNIQUE,
 Telefono NVARCHAR(25) NULL,
 Correo NVARCHAR(120) NULL
);
IF OBJECT_ID('dbo.Libros','U') IS NULL
CREATE TABLE Libros(
 LibroId INT IDENTITY(1,1) PRIMARY KEY,
 Codigo NVARCHAR(30) NOT NULL UNIQUE,
 Titulo NVARCHAR(180) NOT NULL,
 AutorId INT NOT NULL,
 EditorialId INT NOT NULL,
 AnioPublicacion INT NOT NULL CHECK (AnioPublicacion BETWEEN 0 AND 2100),
 Existencias INT NOT NULL DEFAULT 0 CHECK (Existencias >= 0),
 CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES Autores(AutorId),
 CONSTRAINT FK_Libros_Editoriales FOREIGN KEY (EditorialId) REFERENCES Editoriales(EditorialId)
);
IF OBJECT_ID('dbo.Prestamos','U') IS NULL
CREATE TABLE Prestamos(
 PrestamoId INT IDENTITY(1,1) PRIMARY KEY,
 UsuarioId INT NOT NULL,
 LibroId INT NOT NULL,
 FechaPrestamo DATE NOT NULL,
 FechaLimite DATE NOT NULL,
 FechaDevolucion DATE NULL,
 Estado NVARCHAR(15) NOT NULL DEFAULT 'Prestado' CHECK (Estado IN ('Prestado','Devuelto')),
 CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
 CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (LibroId) REFERENCES Libros(LibroId),
 CONSTRAINT CK_Prestamos_Fechas CHECK (FechaLimite >= FechaPrestamo),
 CONSTRAINT CK_Prestamos_Estado CHECK ((Estado='Prestado' AND FechaDevolucion IS NULL) OR (Estado='Devuelto' AND FechaDevolucion IS NOT NULL))
);
GO
IF NOT EXISTS (SELECT 1 FROM Autores)
INSERT INTO Autores(Nombre,Apellido) VALUES
('Gabriel','García Márquez'),('Antoine','de Saint-Exupéry'),('Jane','Austen');
IF NOT EXISTS (SELECT 1 FROM Editoriales)
INSERT INTO Editoriales(Nombre,Ciudad) VALUES
('Editorial Planeta','Bogotá'),('Salamandra','Barcelona'),('Penguin Random House','Bogotá');
IF NOT EXISTS (SELECT 1 FROM Usuarios)
INSERT INTO Usuarios(Nombre,Apellido,Documento,Telefono,Correo) VALUES
('Laura','Gómez','1001001001','3000000001','laura@example.com'),
('Andrés','Pérez','1002002002','3000000002','andres@example.com');
IF NOT EXISTS (SELECT 1 FROM Libros)
INSERT INTO Libros(Codigo,Titulo,AutorId,EditorialId,AnioPublicacion,Existencias) VALUES
('978-0307474728','Cien años de soledad',1,1,1967,5),
('978-0156012195','El principito',2,2,1943,4),
('978-0141439518','Orgullo y prejuicio',3,3,1813,3);
GO
-- Los préstamos se registran desde la aplicación para actualizar existencias en una transacción.
