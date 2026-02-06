-- =============================================================================
-- proc_tinh_chi_phi_co_ban - Tính chi phí cõ b?n c?a t?t c? chuy?n xe
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.CHI_PHI_CO_BAN
-- Parameters: Không có
-- Returns: ma_chuyen, ten_tuyen, chi_phi_nhien_lieu, chi_phi_van_hanh, tong_chi_phi
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_chi_phi_co_ban //

CREATE PROCEDURE proc_tinh_chi_phi_co_ban()
BEGIN
    SELECT 
        cx.ma_chuyen,
        td.ten_tuyen,
        td.khoang_cach,
        x.muc_tieu_hao,
        -- Chi phí nhiên li?u = Kho?ng cách * M?c tiêu hao * Giá nhiên li?u (gi? s? 25000/lít)
        (td.khoang_cach * x.muc_tieu_hao * 25000) AS chi_phi_nhien_lieu,
        -- Chi phí v?n hành = Ph? thu phí v?n hành c?a xe
        x.phu_thu_phi_van_hanh AS chi_phi_van_hanh,
        -- T?ng chi phí
        (td.khoang_cach * x.muc_tieu_hao * 25000 + x.phu_thu_phi_van_hanh) AS tong_chi_phi
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    ORDER BY cx.ma_chuyen;
END //

DELIMITER ;
