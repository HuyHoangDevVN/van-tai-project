-- =============================================================================
-- proc_cap_nhat_bao_tri - C?p nh?t b?n ghi b?o tr?
-- =============================================================================
-- Module: MAINTENANCE
-- Function Key: FunctionKeys.Maintenance.UPDATE
-- Parameters: p_ma_bao_tri, p_ngay_bao_tri, p_noi_dung, p_chi_phi
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_bao_tri //

CREATE PROCEDURE proc_cap_nhat_bao_tri(
    IN p_ma_bao_tri INT,
    IN p_ngay_bao_tri DATETIME,
    IN p_noi_dung TEXT,
    IN p_chi_phi DECIMAL(15,2)
)
BEGIN
    -- Ki?m tra b?n ghi t?n t?i
    IF NOT EXISTS (SELECT 1 FROM bao_tri WHERE ma_bao_tri = p_ma_bao_tri) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'B?n ghi b?o tr? không t?n t?i';
    END IF;

    -- C?p nh?t
    UPDATE bao_tri
    SET 
        ngay_bao_tri = COALESCE(p_ngay_bao_tri, ngay_bao_tri),
        noi_dung = COALESCE(p_noi_dung, noi_dung),
        chi_phi = COALESCE(p_chi_phi, chi_phi)
    WHERE ma_bao_tri = p_ma_bao_tri;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
