-- =============================================================================
-- proc_hoan_thanh_chuyen_xe - Hoàn thành chuy?n xe
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.COMPLETE
-- Parameters: p_ma_chuyen (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_hoan_thanh_chuyen_xe //

CREATE PROCEDURE proc_hoan_thanh_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    DECLARE v_trang_thai_hien_tai VARCHAR(30);

    -- Ki?m tra chuy?n xe t?n t?i
    SELECT trang_thai INTO v_trang_thai_hien_tai
    FROM chuyen_xe 
    WHERE ma_chuyen = p_ma_chuyen;

    IF v_trang_thai_hien_tai IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuy?n xe không t?n t?i';
    END IF;

    -- Ki?m tra tr?ng thái h?p l? ð? hoàn thành
    IF v_trang_thai_hien_tai NOT IN ('Scheduled', 'In Progress') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ch? có th? hoàn thành chuy?n xe ðang lên l?ch ho?c ðang ch?y';
    END IF;

    -- C?p nh?t tr?ng thái thành Completed
    UPDATE chuyen_xe
    SET trang_thai = 'Completed'
    WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
