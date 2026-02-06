-- =============================================================================
-- proc_cap_nhat_khach_hang - C?p nh?t thông tin khách hàng
-- =============================================================================
-- Module: CUSTOMER
-- Function Key: FunctionKeys.Customer.UPDATE
-- Parameters: p_ma_khach_hang, p_ten_khach_hang, p_so_dien_thoai, p_email, p_dia_chi
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_khach_hang //

CREATE PROCEDURE proc_cap_nhat_khach_hang(
    IN p_ma_khach_hang VARCHAR(20),
    IN p_ten_khach_hang VARCHAR(100),
    IN p_so_dien_thoai VARCHAR(15),
    IN p_email VARCHAR(100),
    IN p_dia_chi VARCHAR(200)
)
BEGIN
    -- Ki?m tra khách hàng t?n t?i
    IF NOT EXISTS (SELECT 1 FROM khach_hang WHERE ma_khach_hang = p_ma_khach_hang) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khách hàng không t?n t?i';
    END IF;

    -- Ki?m tra s? ði?n tho?i không trùng v?i khách hàng khác
    IF p_so_dien_thoai IS NOT NULL AND EXISTS (
        SELECT 1 FROM khach_hang WHERE so_dien_thoai = p_so_dien_thoai AND ma_khach_hang != p_ma_khach_hang
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'S? ði?n tho?i ð? ðý?c s? d?ng b?i khách hàng khác';
    END IF;

    -- Ki?m tra email không trùng v?i khách hàng khác
    IF p_email IS NOT NULL AND EXISTS (
        SELECT 1 FROM khach_hang WHERE email = p_email AND ma_khach_hang != p_ma_khach_hang
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Email ð? ðý?c s? d?ng b?i khách hàng khác';
    END IF;

    -- C?p nh?t
    UPDATE khach_hang
    SET 
        ten_khach_hang = COALESCE(p_ten_khach_hang, ten_khach_hang),
        so_dien_thoai = COALESCE(p_so_dien_thoai, so_dien_thoai),
        email = COALESCE(p_email, email),
        dia_chi = COALESCE(p_dia_chi, dia_chi)
    WHERE ma_khach_hang = p_ma_khach_hang;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
