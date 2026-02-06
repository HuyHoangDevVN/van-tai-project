-- =============================================================================
-- proc_xoa_xe - Xóa xe (soft delete ho?c hard delete)
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.DELETE
-- Parameters: p_ma_xe (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_xoa_xe //

CREATE PROCEDURE proc_xoa_xe(
    IN p_ma_xe VARCHAR(20)
)
BEGIN
    -- Ki?m tra xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe không t?n t?i';
    END IF;

    -- Ki?m tra xe ðang có chuy?n xe chýa hoàn thành
    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_xe = p_ma_xe 
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Không th? xóa xe ðang có chuy?n xe ho?t ð?ng';
    END IF;

    -- Soft delete: C?p nh?t tr?ng thái thành "Ð? xóa"
    UPDATE xe
    SET trang_thai = 'Ð? xóa'
    WHERE ma_xe = p_ma_xe;

    -- N?u mu?n hard delete, uncomment d?ng dý?i và comment d?ng trên
    -- DELETE FROM xe WHERE ma_xe = p_ma_xe;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
