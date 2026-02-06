-- ============================================================================
-- PRODUCT MANAGEMENT MODULE - DATABASE SCHEMA
-- MySQL Database Setup Script
-- ============================================================================

-- Create database (if not exists)
CREATE DATABASE IF NOT EXISTS product_management;
USE product_management;

-- ============================================================================
-- TABLE: Categories (Reference table for products)
-- ============================================================================
CREATE TABLE IF NOT EXISTS categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    is_active TINYINT(1) DEFAULT 1,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_category_name (category_name),
    INDEX idx_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- TABLE: Products
-- ============================================================================
CREATE TABLE IF NOT EXISTS products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    product_code VARCHAR(50) NOT NULL UNIQUE,
    product_name VARCHAR(200) NOT NULL,
    price DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    category_id INT,
    description TEXT,
    stock_quantity INT DEFAULT 0,
    is_active TINYINT(1) DEFAULT 1,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_date DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_product_code (product_code),
    INDEX idx_product_name (product_name),
    INDEX idx_category_id (category_id),
    INDEX idx_is_active (is_active),
    INDEX idx_created_date (created_date),
    
    CONSTRAINT fk_product_category 
        FOREIGN KEY (category_id) REFERENCES categories(id) 
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- SEED DATA: Categories
-- ============================================================================
INSERT INTO categories (category_name, description) VALUES
('Electronics', 'Electronic devices and accessories'),
('Clothing', 'Apparel and fashion items'),
('Books', 'Books and educational materials'),
('Home & Garden', 'Home improvement and garden supplies'),
('Sports', 'Sports equipment and accessories');

-- ============================================================================
-- SEED DATA: Products
-- ============================================================================
INSERT INTO products (product_code, product_name, price, category_id, description, stock_quantity) VALUES
('ELEC-001', 'Wireless Bluetooth Headphones', 79.99, 1, 'High-quality wireless headphones with noise cancellation', 150),
('ELEC-002', 'USB-C Charging Cable', 12.99, 1, 'Durable USB-C cable, 2 meters length', 500),
('ELEC-003', 'Portable Power Bank 10000mAh', 35.50, 1, 'Compact power bank with fast charging', 200),
('CLOTH-001', 'Cotton T-Shirt', 24.99, 2, 'Comfortable 100% cotton t-shirt', 300),
('CLOTH-002', 'Denim Jeans', 59.99, 2, 'Classic fit denim jeans', 180),
('BOOK-001', 'Clean Code by Robert Martin', 45.00, 3, 'A handbook of agile software craftsmanship', 75),
('BOOK-002', 'Design Patterns', 55.00, 3, 'Elements of Reusable Object-Oriented Software', 60),
('HOME-001', 'LED Desk Lamp', 32.99, 4, 'Adjustable LED lamp with multiple brightness levels', 120),
('SPORT-001', 'Yoga Mat', 28.50, 5, 'Non-slip yoga mat, 6mm thick', 90);

-- ============================================================================
-- STORED PROCEDURE: sp_Product_GetPaging
-- Retrieves paginated list of products with optional filtering
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_Product_GetPaging;

DELIMITER //

CREATE PROCEDURE sp_Product_GetPaging(
    IN p_Keyword VARCHAR(200),
    IN p_CategoryId INT,
    IN p_IsActive TINYINT,
    IN p_PageIndex INT,
    IN p_PageSize INT,
    OUT p_TotalRecord BIGINT
)
BEGIN
    DECLARE v_Offset INT;
    
    -- Calculate offset for pagination (PageIndex is 1-based)
    SET v_Offset = (p_PageIndex - 1) * p_PageSize;
    
    -- Ensure valid pagination values
    IF v_Offset < 0 THEN
        SET v_Offset = 0;
    END IF;
    
    IF p_PageSize <= 0 THEN
        SET p_PageSize = 20;
    END IF;

    -- Get total count matching the filter criteria
    SELECT COUNT(*) INTO p_TotalRecord
    FROM products p
    WHERE 
        (p_Keyword IS NULL OR p_Keyword = '' 
            OR p.product_name LIKE CONCAT('%', p_Keyword, '%')
            OR p.product_code LIKE CONCAT('%', p_Keyword, '%'))
        AND (p_CategoryId IS NULL OR p_CategoryId = 0 OR p.category_id = p_CategoryId)
        AND (p_IsActive IS NULL OR p.is_active = p_IsActive);

    -- Return paginated results
    SELECT 
        p.id,
        p.product_code,
        p.product_name,
        p.price,
        p.category_id,
        c.category_name,
        p.description,
        p.stock_quantity,
        p.is_active,
        p.created_date,
        p.updated_date
    FROM products p
    LEFT JOIN categories c ON p.category_id = c.id
    WHERE 
        (p_Keyword IS NULL OR p_Keyword = '' 
            OR p.product_name LIKE CONCAT('%', p_Keyword, '%')
            OR p.product_code LIKE CONCAT('%', p_Keyword, '%'))
        AND (p_CategoryId IS NULL OR p_CategoryId = 0 OR p.category_id = p_CategoryId)
        AND (p_IsActive IS NULL OR p.is_active = p_IsActive)
    ORDER BY p.created_date DESC
    LIMIT p_PageSize OFFSET v_Offset;

END //

DELIMITER ;

-- ============================================================================
-- STORED PROCEDURE: sp_Product_GetById
-- Retrieves a single product by ID
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_Product_GetById;

DELIMITER //

CREATE PROCEDURE sp_Product_GetById(
    IN p_Id INT
)
BEGIN
    SELECT 
        p.id,
        p.product_code,
        p.product_name,
        p.price,
        p.category_id,
        c.category_name,
        p.description,
        p.stock_quantity,
        p.is_active,
        p.created_date,
        p.updated_date
    FROM products p
    LEFT JOIN categories c ON p.category_id = c.id
    WHERE p.id = p_Id;
END //

DELIMITER ;

-- ============================================================================
-- STORED PROCEDURE: sp_Product_Create
-- Creates a new product with validation
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_Product_Create;

DELIMITER //

CREATE PROCEDURE sp_Product_Create(
    IN p_ProductCode VARCHAR(50),
    IN p_ProductName VARCHAR(200),
    IN p_Price DECIMAL(18, 2),
    IN p_CategoryId INT,
    IN p_Description TEXT,
    IN p_StockQuantity INT,
    OUT p_ResponseCode INT,
    OUT p_ResponseMessage VARCHAR(500),
    OUT p_LastInsertId BIGINT
)
BEGIN
    -- Declare handler for duplicate entry (product_code is unique)
    DECLARE EXIT HANDLER FOR 1062
    BEGIN
        SET p_ResponseCode = -4;  -- Duplicate error code
        SET p_ResponseMessage = 'Product code already exists';
        SET p_LastInsertId = 0;
    END;
    
    -- Declare handler for foreign key constraint (invalid category_id)
    DECLARE EXIT HANDLER FOR 1452
    BEGIN
        SET p_ResponseCode = -2;  -- Validation error code
        SET p_ResponseMessage = 'Invalid category ID';
        SET p_LastInsertId = 0;
    END;
    
    -- Declare general error handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_ResponseCode = -1;  -- General error code
        SET p_ResponseMessage = 'An error occurred while creating the product';
        SET p_LastInsertId = 0;
    END;

    -- Validate required fields
    IF p_ProductCode IS NULL OR TRIM(p_ProductCode) = '' THEN
        SET p_ResponseCode = -2;
        SET p_ResponseMessage = 'Product code is required';
        SET p_LastInsertId = 0;
    ELSEIF p_ProductName IS NULL OR TRIM(p_ProductName) = '' THEN
        SET p_ResponseCode = -2;
        SET p_ResponseMessage = 'Product name is required';
        SET p_LastInsertId = 0;
    ELSEIF p_Price < 0 THEN
        SET p_ResponseCode = -2;
        SET p_ResponseMessage = 'Price cannot be negative';
        SET p_LastInsertId = 0;
    ELSE
        -- Insert the product
        INSERT INTO products (
            product_code,
            product_name,
            price,
            category_id,
            description,
            stock_quantity,
            is_active,
            created_date
        ) VALUES (
            TRIM(p_ProductCode),
            TRIM(p_ProductName),
            COALESCE(p_Price, 0),
            NULLIF(p_CategoryId, 0),
            p_Description,
            COALESCE(p_StockQuantity, 0),
            1,
            NOW()
        );
        
        SET p_LastInsertId = LAST_INSERT_ID();
        SET p_ResponseCode = 0;  -- Success code
        SET p_ResponseMessage = 'Product created successfully';
    END IF;

END //

DELIMITER ;

-- ============================================================================
-- STORED PROCEDURE: sp_Product_Update
-- Updates an existing product
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_Product_Update;

DELIMITER //

CREATE PROCEDURE sp_Product_Update(
    IN p_Id INT,
    IN p_ProductCode VARCHAR(50),
    IN p_ProductName VARCHAR(200),
    IN p_Price DECIMAL(18, 2),
    IN p_CategoryId INT,
    IN p_Description TEXT,
    IN p_StockQuantity INT,
    IN p_IsActive TINYINT,
    OUT p_ResponseCode INT,
    OUT p_ResponseMessage VARCHAR(500)
)
BEGIN
    DECLARE v_Exists INT DEFAULT 0;
    
    -- Check if product exists
    SELECT COUNT(*) INTO v_Exists FROM products WHERE id = p_Id;
    
    IF v_Exists = 0 THEN
        SET p_ResponseCode = -3;  -- Not found code
        SET p_ResponseMessage = 'Product not found';
    ELSE
        UPDATE products SET
            product_code = COALESCE(NULLIF(TRIM(p_ProductCode), ''), product_code),
            product_name = COALESCE(NULLIF(TRIM(p_ProductName), ''), product_name),
            price = COALESCE(p_Price, price),
            category_id = CASE WHEN p_CategoryId = 0 THEN NULL ELSE COALESCE(p_CategoryId, category_id) END,
            description = COALESCE(p_Description, description),
            stock_quantity = COALESCE(p_StockQuantity, stock_quantity),
            is_active = COALESCE(p_IsActive, is_active),
            updated_date = NOW()
        WHERE id = p_Id;
        
        SET p_ResponseCode = 0;
        SET p_ResponseMessage = 'Product updated successfully';
    END IF;

END //

DELIMITER ;

-- ============================================================================
-- STORED PROCEDURE: sp_Product_Delete
-- Soft deletes a product (sets is_active = 0)
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_Product_Delete;

DELIMITER //

CREATE PROCEDURE sp_Product_Delete(
    IN p_Id INT,
    OUT p_ResponseCode INT,
    OUT p_ResponseMessage VARCHAR(500)
)
BEGIN
    DECLARE v_Exists INT DEFAULT 0;
    
    SELECT COUNT(*) INTO v_Exists FROM products WHERE id = p_Id;
    
    IF v_Exists = 0 THEN
        SET p_ResponseCode = -3;
        SET p_ResponseMessage = 'Product not found';
    ELSE
        UPDATE products SET is_active = 0, updated_date = NOW() WHERE id = p_Id;
        SET p_ResponseCode = 0;
        SET p_ResponseMessage = 'Product deleted successfully';
    END IF;

END //

DELIMITER ;

-- ============================================================================
-- TEST QUERIES
-- ============================================================================
-- Test sp_Product_GetPaging
-- CALL sp_Product_GetPaging(NULL, NULL, 1, 1, 10, @total);
-- SELECT @total AS TotalRecords;

-- Test sp_Product_Create
-- CALL sp_Product_Create('TEST-001', 'Test Product', 99.99, 1, 'Test description', 10, @code, @msg, @id);
-- SELECT @code AS ResponseCode, @msg AS ResponseMessage, @id AS NewId;
