-- =============================================================================
-- proc_cap_nhat_tai_xe - C?p nh?t thông tin tài x?
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.UPDATE
-- Parameters: p_ma_tai_xe, p_ten_tai_xe, p_ngay_sinh, p_gioi_tinh, p_que_quan,
--             p_ton_giao, p_so_cccd, p_ngay_ky_hop_dong, p_he_so_luong
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_tai_xe //

CREATE PROCEDURE proc_cap_nhat_tai_xe(
    IN p_ma_tai_xe VARCHAR(20),
    IN p_ten_tai_xe VARCHAR(100),
    IN p_ngay_sinh DATE,
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_ton_giao VARCHAR(50),
    IN p_so_cccd VARCHAR(20),
    IN p_ngay_ky_hop_dong DATE,
    IN p_he_so_luong DECIMAL(5,2)
)
BEGIN
    -- Ki?m tra tài x? t?n t?i
    IF NOT EXISTS (SELECT 1 FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tài x? không t?n t?i';
    END IF;

    -- Ki?m tra CCCD không trùng v?i tài x? khác
    IF p_so_cccd IS NOT NULL AND EXISTS (
        SELECT 1 FROM tai_xe WHERE so_cccd = p_so_cccd AND ma_tai_xe != p_ma_tai_xe
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'S? CCCD ð? ðý?c s? d?ng b?i tài x? khác';
    END IF;

    -- C?p nh?t
    UPDATE tai_xe
    SET 
        ten_tai_xe = COALESCE(p_ten_tai_xe, ten_tai_xe),
        ngay_sinh = COALESCE(p_ngay_sinh, ngay_sinh),
        gioi_tinh = COALESCE(p_gioi_tinh, gioi_tinh),
        que_quan = COALESCE(p_que_quan, que_quan),
        ton_giao = COALESCE(p_ton_giao, ton_giao),
        so_cccd = COALESCE(p_so_cccd, so_cccd),
        ngay_ky_hop_dong = COALESCE(p_ngay_ky_hop_dong, ngay_ky_hop_dong),
        tuoi = CASE WHEN p_ngay_sinh IS NOT NULL THEN TIMESTAMPDIFF(YEAR, p_ngay_sinh, CURDATE()) ELSE tuoi END,
        he_so_luong = COALESCE(p_he_so_luong, he_so_luong)
    WHERE ma_tai_xe = p_ma_tai_xe;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
