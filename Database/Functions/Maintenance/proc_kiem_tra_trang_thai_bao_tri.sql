-- =============================================================================
-- proc_kiem_tra_trang_thai_bao_tri - Ki?m tra tr?ng thái b?o tr? xe
-- =============================================================================
-- Module: MAINTENANCE
-- Function Key: FunctionKeys.Maintenance.STATUS_CHECK
-- Parameters: p_ma_xe (VARCHAR)
-- Returns: ma_xe, bien_so, ten_xe, trang_thai_bao_tri, ngay_bao_tri_gan_nhat, ngay_bao_tri_tiep_theo
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_kiem_tra_trang_thai_bao_tri //

CREATE PROCEDURE proc_kiem_tra_trang_thai_bao_tri(
    IN p_ma_xe VARCHAR(20)
)
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        x.trang_thai,
        bt.ngay_bao_tri AS ngay_bao_tri_gan_nhat,
        bt.noi_dung AS noi_dung_bao_tri,
        bt.chi_phi AS chi_phi_bao_tri,
        DATE_ADD(bt.ngay_bao_tri, INTERVAL 6 MONTH) AS ngay_bao_tri_tiep_theo,
        CASE 
            WHEN bt.ngay_bao_tri IS NULL THEN 'Chýa có l?ch s? b?o tr?'
            WHEN DATE_ADD(bt.ngay_bao_tri, INTERVAL 6 MONTH) < CURDATE() THEN 'C?n b?o tr?'
            WHEN DATE_ADD(bt.ngay_bao_tri, INTERVAL 5 MONTH) < CURDATE() THEN 'S?p ð?n h?n b?o tr?'
            ELSE 'B?nh thý?ng'
        END AS trang_thai_bao_tri
    FROM xe x
    LEFT JOIN (
        SELECT ma_xe, ngay_bao_tri, noi_dung, chi_phi
        FROM bao_tri
        WHERE (ma_xe, ngay_bao_tri) IN (
            SELECT ma_xe, MAX(ngay_bao_tri)
            FROM bao_tri
            GROUP BY ma_xe
        )
    ) bt ON x.ma_xe = bt.ma_xe
    WHERE (p_ma_xe IS NULL OR x.ma_xe = p_ma_xe)
    ORDER BY x.ma_xe;
END //

DELIMITER ;
