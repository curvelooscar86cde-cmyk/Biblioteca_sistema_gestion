CREATE DATABASE Biblioteca;
GO

USE Biblioteca;
GO

CREATE TABLE Autores (
    ID_Autor INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    Apellidos NVARCHAR(50) NOT NULL,
    Nacionalidad NVARCHAR(50) NOT NULL,
    Fecha_Nacimiento DATE NOT NULL
);
GO

CREATE TABLE Categorias (
    ID_Categoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Libros (
    ID_Libro INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL UNIQUE,
    Titulo NVARCHAR(150) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL UNIQUE,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Disponible INT NOT NULL DEFAULT 0,
    ID_Autor INT NOT NULL,
    ID_Categoria INT NOT NULL,
    CONSTRAINT FK_Libros_Autor FOREIGN KEY (ID_Autor) REFERENCES Autores(ID_Autor),
    CONSTRAINT FK_Libros_Categoria FOREIGN KEY (ID_Categoria) REFERENCES Categorias(ID_Categoria)
);
GO

CREATE TABLE Usuarios (
    ID_Usuario INT IDENTITY(1,1) PRIMARY KEY,
    Documento NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(50) NOT NULL,
    Apellidos NVARCHAR(50) NOT NULL,
    Telefono NVARCHAR(20) NOT NULL,
    Correo NVARCHAR(100) NOT NULL,
    Programa_Academico NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Prestamos (
    ID_Prestamo INT IDENTITY(1,1) PRIMARY KEY,
    ID_Usuario INT NOT NULL,
    Fecha_Prestamo DATE NOT NULL DEFAULT GETDATE(),
    Fecha_Devolucion_Esperada DATE NOT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT FK_Prestamos_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE TABLE DetallePrestamos (
    ID_Detalle INT IDENTITY(1,1) PRIMARY KEY,
    ID_Prestamo INT NOT NULL,
    ID_Libro INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    CONSTRAINT FK_Detalle_Prestamo FOREIGN KEY (ID_Prestamo) REFERENCES Prestamos(ID_Prestamo),
    CONSTRAINT FK_Detalle_Libro FOREIGN KEY (ID_Libro) REFERENCES Libros(ID_Libro)
);
GO




