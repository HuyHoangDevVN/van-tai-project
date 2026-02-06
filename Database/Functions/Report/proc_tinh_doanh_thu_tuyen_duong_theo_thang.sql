-- =============================================================================
-- proc_tinh_doanh_thu_tuyen_duong_theo_thang - Tính doanh thu tuyến đường theo tháng
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.DOANH_THU_TUYEN_DUONG_THANG
-- Parameters: p_tu (DATETIME), p_den (DATETIME)
-- Returns: ma_tuyen, ten_tuyen, thang, doanh_thu_thang
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_tuyen_duong_theo_thang //

CREATE PROCEDURE proc_tinh_doanh_thu_tuyen_duong_theo_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    WITH
    cp AS (
      SELECT
        cx.ma_chuyen,
        cx.ma_xe,
        (xe.muc_tieu_hao/100 * td.khoang_cach * 25000 * dpt.ty_le_chi_phi_van_hanh_tang_them
         + xe.phu_thu_phi_van_hanh) AS chi_phi_co_ban
      FROM chuyen_xe cx
      JOIN xe ON xe.ma_xe = cx.ma_xe
      JOIN tuyen_duong td ON td.ma_tuyen = cx.ma_tuyen
      JOIN do_phuc_tap dpt ON dpt.ma_do_phuc_tap = td.ma_do_phuc_tap
    ),
    le_phi AS (
      SELECT plt.ma_chuyen, SUM(l.phi_phu_thu) AS tong_phi_le
      FROM phi_le_tet plt
      JOIN le l ON l.ma_le = plt.ma_le
      GROUP BY plt.ma_chuyen
    ),
    so_ghe AS (
      SELECT ma_xe_bus AS ma_xe, COUNT(*) AS so_ghe
      FROM ghe
      GROUP BY ma_xe_bus
    ),
    gia_ve AS (
      SELECT
        cx.ma_tuyen,
        cx.thoi_gian_khoi_hanh,
        CEIL((
          (cp.chi_phi_co_ban + COALESCE(lp.tong_phi_le, 0) + g.phu_thu_tien_ich)
          * 1.6
          / NULLIF((sg.so_ghe - 2), 0)
        ) / 10000) * 10000 AS gia_ve
      FROM ve v
      JOIN chuyen_xe cx ON cx.ma_chuyen = v.ma_chuyen
      JOIN cp ON cp.ma_chuyen = cx.ma_chuyen
      JOIN so_ghe sg ON sg.ma_xe = cx.ma_xe
      JOIN ghe g ON g.ma_ghe = v.ma_ghe AND g.ma_xe_bus = cx.ma_xe
      LEFT JOIN le_phi lp ON lp.ma_chuyen = cx.ma_chuyen
      WHERE v.ma_ghe IS NOT NULL
    )
    SELECT
      gv.ma_tuyen,
      td.ten_tuyen,
      gv.thang,
      gv.doanh_thu_thang
    FROM (
      SELECT
        ma_tuyen,
        DATE_FORMAT(thoi_gian_khoi_hanh, '%Y-%m') AS thang,
        SUM(gia_ve) AS doanh_thu_thang
      FROM gia_ve
      WHERE thoi_gian_khoi_hanh >= p_tu
        AND thoi_gian_khoi_hanh <  p_den
      GROUP BY ma_tuyen, DATE_FORMAT(thoi_gian_khoi_hanh, '%Y-%m')
    ) gv
    JOIN tuyen_duong td ON td.ma_tuyen = gv.ma_tuyen
    ORDER BY gv.thang, gv.ma_tuyen;
END //

DELIMITER ;
