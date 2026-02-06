-- =============================================================================
-- proc_tao_bao_tri - T?o m?i b?n ghi b?o tr?
-- =============================================================================
-- Module: MAINTENANCE
-- Function Key: FunctionKeys.Maintenance.CREATE
-- Parameters: p_ma_xe, p_ngay_bao_tri, p_noi_dung, p_chi_phi
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_bao_tri //

CREATE PROCEDURE proc_tao_bao_tri(
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_bao_tri DATETIME,
    IN p_noi_dung TEXT,
    IN p_chi_phi DECIMAL(15,2)
)
BEGIN
    -- Ki?m tra xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe không t?n t?i';
    END IF;

    -- Thêm b?n ghi b?o tr?
    INSERT INTO bao_tri (ma_xe, ngay_bao_tri, noi_dung, chi_phi)
    VALUES (p_ma_xe, p_ngay_bao_tri, p_noi_dung, p_chi_phi);

    -- C?p nh?t tr?ng thái xe (n?u c?n)
    UPDATE xe 
    SET trang_thai = 'Ðang b?o tr?'
    WHERE ma_xe = p_ma_xe 
        AND p_ngay_bao_tri = CURDATE();

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
