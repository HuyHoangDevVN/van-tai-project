-- =============================================================================
-- proc_tao_chuyen_xe - T?o m?i chuy?n xe
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.CREATE
-- Parameters: p_ma_chuyen, p_ma_tuyen, p_ma_xe, p_ngay_khoi_hanh, p_trang_thai
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_chuyen_xe //

CREATE PROCEDURE proc_tao_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_tuyen VARCHAR(20),
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_khoi_hanh DATETIME,
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    -- Ki?m tra tuy?n ðý?ng t?n t?i
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuy?n ðý?ng không t?n t?i';
    END IF;

    -- Ki?m tra xe t?n t?i và ðang ho?t ð?ng
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe AND trang_thai = 'Ho?t ð?ng') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe không t?n t?i ho?c không ðang ho?t ð?ng';
    END IF;

    -- Ki?m tra xe không có chuy?n khác trong cùng th?i ði?m
    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_xe = p_ma_xe 
        AND DATE(ngay_khoi_hanh) = DATE(p_ngay_khoi_hanh)
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe ð? có chuy?n khác trong ngày này';
    END IF;

    -- Thêm chuy?n xe m?i
    INSERT INTO chuyen_xe (
        ma_chuyen, ma_tuyen, ma_xe, ngay_khoi_hanh, trang_thai
    )
    VALUES (
        p_ma_chuyen, p_ma_tuyen, p_ma_xe, p_ngay_khoi_hanh, COALESCE(p_trang_thai, 'Scheduled')
    );

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
