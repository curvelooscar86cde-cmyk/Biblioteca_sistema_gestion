# Sistema de Gestión de Biblioteca

Aplicación de escritorio para automatizar la administración de una biblioteca institucional: gestión de libros, autores, categorías, usuarios, préstamos y devoluciones.

Proyecto ACA de la asignatura **Programación Avanzada** — Corporación Unificada Nacional de Educación Superior (CUN).

## Tecnologías utilizadas

- **C# / .NET 8**
- **Windows Forms** (interfaz gráfica)
- **SQL Server Express** (base de datos)
- **ADO.NET** (`Microsoft.Data.SqlClient`) para el acceso a datos

## Arquitectura

El proyecto sigue una arquitectura por capas:

```
Presentación (Windows Forms)
Lógica de Negocio (Servicios, validaciones, excepciones de negocio)
Acceso a Datos (Repositorios, ADO.NET)
Base de Datos (SQL Server)
```

### Estructura de carpetas

```
Biblioteca/
├── Program.cs                
├── Dominio/                    
├── AccesoDatos/
│   ├── Conexion/         
│   ├── Contratos/             
│   └── Repositorios/           
├── LogicaNegocio/
│   ├── Contratos/               
│   ├── Servicios/             
│   └── Excepciones/            
└── Presentacion/                 
```

## Funcionalidades

- **Gestión de Libros**: registrar, consultar, editar, eliminar, buscar por título/autor/categoría, control de disponibilidad.
- **Gestión de Autores** y **Categorías**: CRUD completo.
- **Gestión de Usuarios**: CRUD con validación de documento único y formato de correo.
- **Préstamos y Devoluciones**: registro de préstamos con validación de existencias disponibles, y devoluciones que actualizan automáticamente la disponibilidad.

## Reglas de negocio implementadas

- No se permite registrar libros con ISBN duplicado.
- No se permite registrar usuarios con documento duplicado.
- No se permite prestar un libro sin existencias disponibles.
- Las devoluciones actualizan automáticamente la disponibilidad de cada libro.

## Cómo ejecutar el proyecto

1. Clonar el repositorio.
2. Abrir la carpeta en Visual Studio o Visual Studio Code.
3. Ejecutar los scripts SQL ubicados en `/scripts_SQL` para crear la base de datos, las tablas y los datos de prueba.
4. Ajustar la cadena de conexión en `Program.cs` con el nombre de tu instancia de SQL Server.
5. Ejecutar:
   ```
   dotnet restore
   dotnet build
   dotnet run
   ```

## Base de datos

El modelo relacional contiene las siguientes tablas: `Autores`, `Categorias`, `Libros`, `Usuarios`, `Prestamos`, `DetallePrestamos`, con llaves primarias, foráneas y restricciones de integridad (ISBN único, documento único, cantidad mayor que cero).

## Licencia

Proyecto académico, sin fines comerciales.
