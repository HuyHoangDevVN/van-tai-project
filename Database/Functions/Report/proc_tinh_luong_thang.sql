-- =============================================================================
-- proc_tinh_luong_thang - Tính lương tháng tài xế
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.LUONG_THANG_TAI_XE
-- Parameters: p_tu (DATETIME), p_den (DATETIME)
-- Returns: ma_tai_xe, ten_tai_xe, tong_km, so_tuyen, luong_thang
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_luong_thang //

CREATE PROCEDURE proc_tinh_luong_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        SUM(td.khoang_cach) AS tong_km,
        COUNT(td.ma_tuyen)  AS so_tuyen,
        SUM(
            (vt.phu_cap + tx.he_so_luong * td.khoang_cach)
            * dpt.ty_le_chi_phi_van_hanh_tang_them
        ) AS luong_thang
    FROM tai_xe tx
    JOIN chi_dinh cd     ON cd.ma_tai_xe = tx.ma_tai_xe
    JOIN vi_tri vt       ON vt.ma_vi_tri = cd.ma_vi_tri
    JOIN chuyen_xe cx    ON cx.ma_chuyen = cd.ma_chuyen
    JOIN tuyen_duong td  ON td.ma_tuyen = cx.ma_tuyen
    JOIN do_phuc_tap dpt ON dpt.ma_do_phuc_tap = td.ma_do_phuc_tap
    WHERE cx.thoi_gian_khoi_hanh >= p_tu
      AND cx.thoi_gian_khoi_hanh <  p_den
    GROUP BY tx.ma_tai_xe, tx.ten_tai_xe;
END //

DELIMITER ;
