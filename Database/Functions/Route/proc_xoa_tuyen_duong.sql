-- =============================================================================
-- proc_xoa_tuyen_duong - Xóa tuy?n ðý?ng
-- =============================================================================
-- Module: ROUTE
-- Function Key: FunctionKeys.Route.DELETE
-- Parameters: p_ma_tuyen (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_xoa_tuyen_duong //

CREATE PROCEDURE proc_xoa_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20)
)
BEGIN
    -- Ki?m tra tuy?n ðý?ng t?n t?i
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuy?n ðý?ng không t?n t?i';
    END IF;

    -- Ki?m tra có chuy?n xe ðang ho?t ð?ng
    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_tuyen = p_ma_tuyen 
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Không th? xóa tuy?n ðý?ng ðang có chuy?n xe ho?t ð?ng';
    END IF;

    -- Xóa giá vé liên quan
    DELETE FROM gia WHERE ma_tuyen = p_ma_tuyen;

    -- Xóa tuy?n ðý?ng
    DELETE FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
