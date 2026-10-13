CREATE TABLE `usuario` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `email` varchar(150) UNIQUE NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `url_avatar` varchar (500) NULL DEFAULT NULL,
  `rol` ENUM ('dueno', 'empleado') NOT NULL DEFAULT 'empleado',
  `estado` boolean DEFAULT true
);

CREATE TABLE `insumo` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `unidad` varchar(20) COMMENT 'kg, litro, unidad, etc.',
  `costo_unitario` decimal(12,4) NOT NULL
);

CREATE TABLE `producto` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `costo_extra` decimal(12,2) DEFAULT 0 COMMENT 'Mano de obra, packaging, envio, etc.',
  `margen_objetivo` decimal(5,2) NOT NULL COMMENT 'Porcentaje de ganancia deseado',
  `precio_actual` decimal(12,2) NOT NULL,
  `stock_actual` decimal(12,2) DEFAULT 0,
  `url_producto` varchar (500) NULL DEFAULT NULL,
  `estado` boolean DEFAULT true
);

CREATE TABLE `producto_insumo` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `producto_id` int NOT NULL,
  `insumo_id` int NOT NULL,
  `cantidad` decimal(12,4) NOT NULL COMMENT 'Insumo por unidad de producto'
);

CREATE TABLE `cliente` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `telefono` varchar(30),
  `email` varchar(150)
);

CREATE TABLE `venta` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `cliente_id` int NOT NULL COMMENT 'Opcional: venta de mostrador',
  `usuario_id` int NOT NULL COMMENT 'Quien registro la venta',
  `fecha` timestamp DEFAULT (now()),
  `total` decimal(12,2) NOT NULL,
  `estado` boolean DEFAULT true
);

CREATE TABLE `venta_item` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `venta_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `cantidad` decimal(12,2) NOT NULL,
  `precio_momento` decimal(12,2) NOT NULL COMMENT 'Precio al momento de la venta',
  `costo_momento` decimal(12,2) NOT NULL COMMENT 'Costo al momento de la venta'
);

CREATE TABLE `gasto` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `usuario_id` int NOT NULL COMMENT 'Quien registro el gasto',
  `descripcion` varchar(200) NOT NULL,
  `categoria` varchar(100) COMMENT 'Texto libre: alquiler, luz, publicidad',
  `monto` decimal(12,2) NOT NULL,
  `fecha` date NOT NULL
);

CREATE UNIQUE INDEX `producto_insumo_index_0` ON `producto_insumo` (`producto_id`, `insumo_id`);

ALTER TABLE `producto_insumo` ADD FOREIGN KEY (`producto_id`) REFERENCES `producto` (`id`);

ALTER TABLE `producto_insumo` ADD FOREIGN KEY (`insumo_id`) REFERENCES `insumo` (`id`);

ALTER TABLE `venta` ADD FOREIGN KEY (`cliente_id`) REFERENCES `cliente` (`id`);

ALTER TABLE `venta` ADD FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`id`);

ALTER TABLE `venta_item` ADD FOREIGN KEY (`venta_id`) REFERENCES `venta` (`id`);

ALTER TABLE `venta_item` ADD FOREIGN KEY (`producto_id`) REFERENCES `producto` (`id`);

ALTER TABLE `gasto` ADD FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`id`);

ALTER TABLE `venta` ADD FOREIGN KEY (`total`) REFERENCES `venta` (`id`);
