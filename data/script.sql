-- GIPE - Esquema de base de datos (MySQL 8+/9+)
-- Ejecutar con: mysql -u root -p < data/script.sql
-- Es idempotente: no borra datos existentes.
-- Para recrear todo desde cero usar data/reset.sql y volver a ejecutar este script.

CREATE DATABASE IF NOT EXISTS bdgipe
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE bdgipe;

CREATE TABLE IF NOT EXISTS usuario (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nombre VARCHAR(150) NOT NULL,
  email VARCHAR(150) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  url_avatar VARCHAR(500) NULL DEFAULT NULL,
  rol ENUM('Dueno', 'Empleado') NOT NULL DEFAULT 'Empleado',
  estado BOOLEAN NOT NULL DEFAULT TRUE,
  UNIQUE KEY uq_usuario_email (email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS insumo (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nombre VARCHAR(150) NOT NULL,
  unidad ENUM('Unidad', 'Kilogramo', 'Litro', 'Metro', 'Otro') NOT NULL DEFAULT 'Unidad',
  costo_unitario DECIMAL(12,4) NOT NULL DEFAULT 0,
  estado BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS producto (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nombre VARCHAR(150) NOT NULL,
  descripcion VARCHAR(500) NULL DEFAULT NULL,
  costo_extra DECIMAL(12,2) NOT NULL DEFAULT 0,
  margen_objetivo DECIMAL(5,2) NOT NULL DEFAULT 0,
  precio_actual DECIMAL(12,2) NOT NULL DEFAULT 0,
  stock_actual DECIMAL(12,2) NOT NULL DEFAULT 0,
  url_producto VARCHAR(500) NULL DEFAULT NULL,
  estado BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS producto_insumo (
  id INT PRIMARY KEY AUTO_INCREMENT,
  producto_id INT NOT NULL,
  insumo_id INT NOT NULL,
  cantidad DECIMAL(12,4) NOT NULL,
  UNIQUE KEY uq_producto_insumo (producto_id, insumo_id),
  CONSTRAINT fk_producto_insumo_producto FOREIGN KEY (producto_id) REFERENCES producto (id),
  CONSTRAINT fk_producto_insumo_insumo FOREIGN KEY (insumo_id) REFERENCES insumo (id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS cliente (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nombre VARCHAR(150) NOT NULL,
  apellido VARCHAR(150) NULL DEFAULT NULL,
  telefono VARCHAR(30) NULL DEFAULT NULL,
  email VARCHAR(150) NULL DEFAULT NULL,
  direccion VARCHAR(200) NULL DEFAULT NULL,
  estado BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS venta (
  id INT PRIMARY KEY AUTO_INCREMENT,
  cliente_id INT NULL DEFAULT NULL,
  usuario_id INT NOT NULL,
  fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  monto_total DECIMAL(12,2) NOT NULL DEFAULT 0,
  estado BOOLEAN NOT NULL DEFAULT TRUE,
  KEY idx_venta_fecha (fecha),
  KEY idx_venta_usuario (usuario_id),
  CONSTRAINT fk_venta_cliente FOREIGN KEY (cliente_id) REFERENCES cliente (id),
  CONSTRAINT fk_venta_usuario FOREIGN KEY (usuario_id) REFERENCES usuario (id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS venta_item (
  id INT PRIMARY KEY AUTO_INCREMENT,
  venta_id INT NOT NULL,
  producto_id INT NOT NULL,
  cantidad DECIMAL(12,2) NOT NULL,
  precio_momento DECIMAL(12,2) NOT NULL,
  costo_momento DECIMAL(12,2) NOT NULL,
  KEY idx_venta_item_venta (venta_id),
  CONSTRAINT fk_venta_item_venta FOREIGN KEY (venta_id) REFERENCES venta (id) ON DELETE CASCADE,
  CONSTRAINT fk_venta_item_producto FOREIGN KEY (producto_id) REFERENCES producto (id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS gasto (
  id INT PRIMARY KEY AUTO_INCREMENT,
  usuario_id INT NOT NULL,
  descripcion VARCHAR(200) NOT NULL,
  categoria ENUM('Alquiler', 'Servicios', 'Publicidad', 'Transporte', 'Otro') NOT NULL DEFAULT 'Otro',
  monto DECIMAL(12,2) NOT NULL DEFAULT 0,
  fecha DATE NOT NULL,
  KEY idx_gasto_fecha (fecha),
  CONSTRAINT fk_gasto_usuario FOREIGN KEY (usuario_id) REFERENCES usuario (id)
) ENGINE=InnoDB;
