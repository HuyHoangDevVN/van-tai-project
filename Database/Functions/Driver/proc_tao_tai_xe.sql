-- =============================================================================
-- proc_tao_tai_xe - T?o m?i tài x?
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.CREATE
-- Parameters: p_ma_tai_xe, p_ten_tai_xe, p_ngay_sinh, p_gioi_tinh, p_que_quan,
--             p_ton_giao, p_so_cccd, p_ngay_ky_hop_dong, p_he_so_luong
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_tai_xe //

CREATE PROCEDURE proc_tao_tai_xe(
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
    -- Ki?m tra CCCD ð? t?n t?i
    IF EXISTS (SELECT 1 FROM tai_xe WHERE so_cccd = p_so_cccd) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'S? CCCD ð? t?n t?i trong h? th?ng';
    END IF;

    -- Tính tu?i
    DECLARE v_tuoi INT;
    SET v_tuoi = TIMESTAMPDIFF(YEAR, p_ngay_sinh, CURDATE());

    -- Ki?m tra tu?i h?p l? (18-60)
    IF v_tuoi < 18 OR v_tuoi > 60 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tu?i tài x? ph?i t? 18 ð?n 60';
    END IF;

    -- Thêm tài x? m?i
    INSERT INTO tai_xe (
        ma_tai_xe, ten_tai_xe, ngay_sinh, gioi_tinh, que_quan,
        ton_giao, so_cccd, ngay_ky_hop_dong, tuoi, he_so_luong
    )
    VALUES (
        p_ma_tai_xe, p_ten_tai_xe, p_ngay_sinh, p_gioi_tinh, p_que_quan,
        p_ton_giao, p_so_cccd, p_ngay_ky_hop_dong, v_tuoi, COALESCE(p_he_so_luong, 1.0)
    );

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
