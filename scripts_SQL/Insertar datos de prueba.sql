USE Biblioteca;
GO

INSERT INTO Autores (Nombre, Apellidos, Nacionalidad, Fecha_Nacimiento) VALUES
('Imran', 'Ahmad', 'Pakistán', '1995-08-14'),
('C.', 'Pérez', 'Colombia', '1998-03-22'),
('D.', 'Schmidt', 'Alemania', '1992-11-05'),
('L.', 'Herrero', 'España', '1996-06-18'),
('P.', 'Rodríguez', 'Colombia', '1999-01-30'),
('J.', 'Carnero', 'España', '1994-09-12'),
('L.', 'Socconini', 'Italia', '1995-02-14'),
('L.', 'Arbaiza', 'Perú', '1998-10-06'),
('Kenneth', 'Rosen', 'Estados Unidos', '1950-05-20'),
('James', 'Kurose', 'Estados Unidos', '1956-11-10'),
('Abraham', 'Silberschatz', 'Estados Unidos', '1953-07-03');
GO

INSERT INTO Categorias (Nombre) VALUES
('Programación'),
('Bases de datos'),
('Redes'),
('Matemáticas'),
('Electrónica'),
('Inteligencia Artificial'),
('Otros');
GO

INSERT INTO Libros (Codigo, Titulo, ISBN, Cantidad, Disponible, ID_Autor, ID_Categoria) VALUES
('LIB001', '50 algoritmos que todo programador debe conocer', '978-1789801217', 3, 3, 1, 1), 
('LIB002', 'Sistemas de Big Data', '978-8426728451', 4, 4, 2, 2),                             
('LIB003', 'Raspberry Pi', '978-8426728468', 2, 1, 3, 5),                                     
('LIB004', 'Hacking Ético', '978-8426728475', 6, 6, 4, 3),                                    
('LIB005', 'Inteligencia Artificial', '978-8426728482', 4, 3, 5, 6),                          
('LIB006', 'Programación de Servicios y Procesos en Python', '978-8426728499', 3, 2, 6, 1),   
('LIB007', 'Lean Six Sigma Green Belt paso a paso', '978-8494960707', 5, 5, 7, 7),             
('LIB008', 'El desarrollo de la tesis', '978-8417858964', 2, 2, 8, 7),                       
('LIB009', 'Matemáticas Discretas y sus Aplicaciones', '978-6071512965', 3, 2, 9, 4),         
('LIB010', 'Redes de Computadoras: un enfoque descendente', '978-6073239878', 4, 4, 10, 3),   
('LIB011', 'Fundamentos de Bases de Datos', '978-6071511497', 3, 3, 11, 2);                  
GO

INSERT INTO Usuarios (Documento, Nombre, Apellidos, Telefono, Correo, Programa_Academico) VALUES
('1001111111', 'Juan', 'Pérez', '3011111111', 'juan.perez.est@gmail.com', 'Ingeniería en Sistemas'),
('1001111112', 'María', 'López', '3011111112', 'maria.lopez2003@gmail.com', 'Contabilidad'),
('1001111113', 'Carlos', 'Rodríguez', '3011111113', 'carlos.rodriguez.dev@gmail.com', 'Ingeniería Civil'),
('1001111114', 'Andrea', 'Martínez', '3011111114', 'andrea.martinez99@gmail.com', 'Ingeniería en Sistemas'),
('1001111115', 'Sebastián', 'Gómez', '3011111115', 'sebastian.gomez.uni@gmail.com', 'Ingeniería Industrial'),
('1001111116', 'Daniel', 'Herrera', '3011111116', 'daniel.herrera01@gmail.com', 'Arquitectura'),
('1001111117', 'Paula', 'Rincón', '3011111117', 'paula.rincon.acad@gmail.com', 'Ingeniería en Sistemas');
GO

INSERT INTO Prestamos (ID_Usuario, Fecha_Prestamo, Fecha_Devolucion_Esperada, Estado) VALUES
(2, '2026-02-03', '2026-02-05', 'Devuelto'),  
(3, '2026-02-04', '2026-02-07', 'Devuelto'),   
(6, '2026-02-05', '2026-02-10', 'Activo'),    
(5, '2026-02-06', '2026-02-15', 'Atrasado'),  
(7, '2026-02-10', '2026-02-13', 'Devuelto'), 
(1, '2026-02-13', '2026-02-24', 'Atrasado'),   
(4, '2026-02-08', '2026-02-18', 'Activo'),     
(2, '2026-02-09', '2026-02-16', 'Devuelto');  
GO

INSERT INTO DetallePrestamos (ID_Prestamo, ID_Libro, Cantidad) VALUES
(1, 2, 1),
(2, 4, 1),
(3, 3, 1),
(4, 6, 1),
(5, 7, 1),
(6, 5, 1),
(7, 9, 1),
(8, 10, 1);
GO