-- =============================================================================
-- proc_lay_lich_su_bao_tri - L?y l?ch s? b?o tr? xe
-- =============================================================================
-- Module: MAINTENANCE
-- Function Key: FunctionKeys.Maintenance.GET_HISTORY
-- Parameters: p_ma_xe (VARCHAR), p_tu_ngay (DATETIME), p_den_ngay (DATETIME)
-- Returns: ma_bao_tri, ma_xe, bien_so, ngay_bao_tri, noi_dung, chi_phi
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_lich_su_bao_tri //

CREATE PROCEDURE proc_lay_lich_su_bao_tri(
    IN p_ma_xe VARCHAR(20),
    IN p_tu_ngay DATETIME,
    IN p_den_ngay DATETIME
)
BEGIN
    SELECT 
        bt.ma_bao_tri,
        bt.ma_xe,
        x.bien_so,
        x.ten_xe,
        bt.ngay_bao_tri,
        bt.noi_dung,
        bt.chi_phi
    FROM bao_tri bt
    INNER JOIN xe x ON bt.ma_xe = x.ma_xe
    WHERE (p_ma_xe IS NULL OR bt.ma_xe = p_ma_xe)
        AND (p_tu_ngay IS NULL OR bt.ngay_bao_tri >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR bt.ngay_bao_tri <= p_den_ngay)
    ORDER BY bt.ngay_bao_tri DESC;
END //

DELIMITER ;
