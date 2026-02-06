-- =============================================================================
-- proc_cap_nhat_chuyen_xe - C?p nh?t chuy?n xe
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.UPDATE
-- Parameters: p_ma_chuyen, p_ma_tuyen, p_ma_xe, p_ngay_khoi_hanh, p_trang_thai
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_chuyen_xe //

CREATE PROCEDURE proc_cap_nhat_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_tuyen VARCHAR(20),
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_khoi_hanh DATETIME,
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    -- Ki?m tra chuy?n xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuy?n xe không t?n t?i';
    END IF;

    -- C?p nh?t
    UPDATE chuyen_xe
    SET 
        ma_tuyen = COALESCE(p_ma_tuyen, ma_tuyen),
        ma_xe = COALESCE(p_ma_xe, ma_xe),
        ngay_khoi_hanh = COALESCE(p_ngay_khoi_hanh, ngay_khoi_hanh),
        trang_thai = COALESCE(p_trang_thai, trang_thai)
    WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
