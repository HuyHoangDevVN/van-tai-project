-- =============================================================================
-- proc_chi_phi_co_ban - Tính chi phí cơ bản của tất cả chuyến xe
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.CHI_PHI_CO_BAN
-- Parameters: Không có
-- Returns: ma_chuyen, ten_chuyen, ma_xe, khoang_cach, chi_phi_co_ban
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_chi_phi_co_ban //

CREATE PROCEDURE proc_chi_phi_co_ban()
BEGIN
    SELECT
        cx.ma_chuyen,
        cx.ten_chuyen,
        cx.ma_xe,
        td.khoang_cach,
        (xe.muc_tieu_hao/100 * td.khoang_cach * 25000 * dpt.ty_le_chi_phi_van_hanh_tang_them
         + xe.phu_thu_phi_van_hanh) AS chi_phi_co_ban
    FROM chuyen_xe cx
    JOIN xe ON cx.ma_xe = xe.ma_xe
    JOIN tuyen_duong td ON td.ma_tuyen = cx.ma_tuyen
    JOIN do_phuc_tap dpt ON dpt.ma_do_phuc_tap = td.ma_do_phuc_tap;
END //

DELIMITER ;
