-- =============================================================================
-- proc_xoa_tai_xe - Xóa tài x?
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.DELETE
-- Parameters: p_ma_tai_xe (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_xoa_tai_xe //

CREATE PROCEDURE proc_xoa_tai_xe(
    IN p_ma_tai_xe VARCHAR(20)
)
BEGIN
    -- Ki?m tra tài x? t?n t?i
    IF NOT EXISTS (SELECT 1 FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tài x? không t?n t?i';
    END IF;

    -- Ki?m tra tài x? ðang ðý?c phân công
    IF EXISTS (
        SELECT 1 FROM phan_cong_tai_xe pctx
        INNER JOIN chuyen_xe cx ON pctx.ma_xe = cx.ma_xe
        WHERE pctx.ma_tai_xe = p_ma_tai_xe 
        AND cx.trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Không th? xóa tài x? ðang ðý?c phân công chuy?n xe';
    END IF;

    -- Xóa phân công trý?c
    DELETE FROM phan_cong_tai_xe WHERE ma_tai_xe = p_ma_tai_xe;

    -- Xóa tài x?
    DELETE FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
