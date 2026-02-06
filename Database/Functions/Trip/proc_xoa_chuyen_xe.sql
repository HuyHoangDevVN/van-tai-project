-- =============================================================================
-- proc_xoa_chuyen_xe - Xóa chuy?n xe
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.DELETE
-- Parameters: p_ma_chuyen (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_xoa_chuyen_xe //

CREATE PROCEDURE proc_xoa_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    -- Ki?m tra chuy?n xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuy?n xe không t?n t?i';
    END IF;

    -- Ki?m tra có vé ð? bán
    IF EXISTS (SELECT 1 FROM ve WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Không th? xóa chuy?n xe ð? có vé ðý?c ð?t';
    END IF;

    -- Xóa chuy?n xe
    DELETE FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
