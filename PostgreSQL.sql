CREATE TABLE Companies (
    company_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    img TEXT
);

INSERT INTO Companies 
    (name, email, img) 
VALUES
    ('Weyland-Yutani', 'info@wy.com', 'wy.webp'),
    ('Omni Consumer Products', 'info@ocp.com', 'ocp.webp'),
    ('Cyberdyne Systems Corporation', 'info@csc.com', 'csc.webp'),
    ('Umbrella Corporation', 'info@uc.com', 'uc.webp');
    
SELECT * FROM Companies;

CREATE TABLE Products (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    price INT NOT NULL
);

-- Insertar datos para Weyland-Yutani
INSERT INTO Products (name, description, price) VALUES
('Réplica del Cargador de Montacargas P-5000', 'Un juguete funcional a escala de 1:12 con brazos móviles y pinzas. Perfecto para recrear la batalla épica de la Reina Alien.', 55000),
('Set de Miniaturas Coleccionables de Xenomorfos', 'Un paquete de tres figuras de 5 cm de un Facehugger, un Chestburster y un Guerrero Xenomorfo. Ideales para exhibición en escritorio.', 18000),
('Figura de Acción de un Sintético con Daños de Batalla', 'Un androide Weyland-Yutani de 15 cm con una función de "daño" removible que revela el endoesqueleto interior.', 29990);

-- Insertar datos para Omni Consumer Products
INSERT INTO Products (name, description, price) VALUES
('Vehículo de Patrulla OCP con Luces y Sonido', 'Un coche de policía a escala con el logo de OCP, luces parpadeantes y sonidos de sirena.', 22000),
('Figura de RoboCop "Ultimate Edition"', 'Una figura de 18 cm con múltiples puntos de articulación, tres caras intercambiables y su arma "Auto 9".', 45000),
('Set de Construcción "Torre ED-209"', 'Un kit de bloques que te permite armar tu propio robot ED-209. Incluye un mecanismo de disparo de misiles a escala.', 68000);

-- Insertar datos para Cyberdyne Systems Corporation
INSERT INTO Products (name, description, price) VALUES
('Figura de Acción del T-800 Endoskeleton', 'Una figura de 15 cm completamente cromada, con articulaciones de alta calidad y ojos que se iluminan con un botón.', 35000),
('Cabeza de Colección de un T-1000 de Metal Líquido', 'Una réplica a escala 1:1 de la cabeza del T-1000, con una apariencia de metal líquido plateado y detalles de daños.', 75000),
('Réplica del Computador de Skynet', 'Un juguete interactivo con forma de la icónica computadora, que simula el proceso de inicio de un apocalipsis en miniatura.', 50000);

-- Insertar datos para Umbrella Corporation
INSERT INTO Products (name, description, price) VALUES
('Figura de Acción del T-Virus Zombie', 'Una figura de un zombi con un efecto de "piel podrida" y un recipiente con un líquido simulando el T-Virus que brilla en la oscuridad.', 28000),
('Set de Laboratorio Secreto de Umbrella', 'Un playset a pequeña escala de un laboratorio, con tubos de ensayo, un microscopio y una pequeña figura del Birkin G-Virus.', 60000),
('Réplica de la Vacuna Anti-Virus', 'Una ampolla de vidrio de colección que contiene un líquido de color azul, diseñado para parecerse a la cura de la película.', 15000);

SELECT * FROM Products;

CREATE TABLE EstadoPago (
    estado_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO EstadoPago 
    (nombre) 
VALUES
    ('Pendiente'),
    ('Pagado'),
    ('Anulado');

CREATE TABLE Pedidos (
    pedido_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID,
    fecha DATETIME NOT NULL,
    total DECIMAL(10, 2) NOT NULL,
    estado_id UUID,
    FOREIGN KEY (cliente_id) REFERENCES Clientes(cliente_id),
    FOREIGN KEY (estado_id) REFERENCES EstadoPago(estado_id)
);

CREATE TABLE Detalle_Pedido (
    detalle_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    pedido_id UUID,
    producto_id UUID,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10, 2) NOT NULL,
    monto_total DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (pedido_id) REFERENCES Pedidos(pedido_id),
    FOREIGN KEY (producto_id) REFERENCES Productos(producto_id)
);

