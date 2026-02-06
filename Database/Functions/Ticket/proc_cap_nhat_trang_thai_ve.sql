-- =============================================================================
-- proc_cap_nhat_trang_thai_ve - C?p nh?t tr?ng thái vé
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.UPDATE_STATUS
-- Parameters: p_ma_ve (VARCHAR), p_trang_thai (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_trang_thai_ve //

CREATE PROCEDURE proc_cap_nhat_trang_thai_ve(
    IN p_ma_ve VARCHAR(20),
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    -- Ki?m tra vé t?n t?i
    IF NOT EXISTS (SELECT 1 FROM ve WHERE ma_ve = p_ma_ve) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Vé không t?n t?i';
    END IF;

    -- Ki?m tra tr?ng thái h?p l?
    IF p_trang_thai NOT IN ('Booked', 'Confirmed', 'Checked-in', 'Completed', 'Cancelled') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tr?ng thái không h?p l?';
    END IF;

    -- C?p nh?t tr?ng thái
    UPDATE ve
    SET trang_thai = p_trang_thai
    WHERE ma_ve = p_ma_ve;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
