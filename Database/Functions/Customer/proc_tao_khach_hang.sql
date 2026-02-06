-- =============================================================================
-- proc_tao_khach_hang - T?o m?i khách hàng
-- =============================================================================
-- Module: CUSTOMER
-- Function Key: FunctionKeys.Customer.CREATE
-- Parameters: p_ma_khach_hang, p_ten_khach_hang, p_so_dien_thoai, p_email, p_dia_chi
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_khach_hang //

CREATE PROCEDURE proc_tao_khach_hang(
    IN p_ma_khach_hang VARCHAR(20),
    IN p_ten_khach_hang VARCHAR(100),
    IN p_so_dien_thoai VARCHAR(15),
    IN p_email VARCHAR(100),
    IN p_dia_chi VARCHAR(200)
)
BEGIN
    -- Ki?m tra s? ði?n tho?i ð? t?n t?i
    IF EXISTS (SELECT 1 FROM khach_hang WHERE so_dien_thoai = p_so_dien_thoai) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'S? ði?n tho?i ð? ðý?c ðãng k?';
    END IF;

    -- Ki?m tra email ð? t?n t?i (n?u có)
    IF p_email IS NOT NULL AND EXISTS (SELECT 1 FROM khach_hang WHERE email = p_email) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Email ð? ðý?c ðãng k?';
    END IF;

    -- Thêm khách hàng m?i
    INSERT INTO khach_hang (
        ma_khach_hang, ten_khach_hang, so_dien_thoai, email, dia_chi
    )
    VALUES (
        p_ma_khach_hang, p_ten_khach_hang, p_so_dien_thoai, p_email, p_dia_chi
    );

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
