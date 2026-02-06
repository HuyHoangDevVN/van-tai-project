-- =============================================================================
-- proc_gia_ve_xe_bus_ngoi - Lấy giá vé xe bus ngồi theo chuyến
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.GIA_VE_XE_BUS_NGOI
-- Parameters: Không có
-- Returns: stt, ma_chuyen, ten_chuyen, vi_tri, ma_xe, chi_phi_co_ban, khoang_cach, gia_ve
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_gia_ve_xe_bus_ngoi //

CREATE PROCEDURE proc_gia_ve_xe_bus_ngoi()
BEGIN
    -- 1.6 là lợi nhuận mong muốn
    -- -2 là số ghế bị giảm đi theo đề bài 
    WITH cp AS (
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
        JOIN do_phuc_tap dpt ON dpt.ma_do_phuc_tap = td.ma_do_phuc_tap
    ),
    le_phi AS (
        SELECT
            plt.ma_chuyen,
            SUM(l.phi_phu_thu) AS tong_phi_le
        FROM phi_le_tet plt
        JOIN le l ON l.ma_le = plt.ma_le
        GROUP BY plt.ma_chuyen
    ),
    so_ghe AS (
        SELECT ma_xe_bus AS ma_xe, COUNT(*) AS so_ghe
        FROM ghe
        GROUP BY ma_xe_bus
    )
    SELECT
        v.stt,
        cx.ma_chuyen,
        cx.ten_chuyen,
        g.vi_tri,
        cx.ma_xe,
        cp.chi_phi_co_ban,
        cp.khoang_cach,
        CEIL(
            (
                (cp.chi_phi_co_ban + COALESCE(lp.tong_phi_le, 0) + g.phu_thu_tien_ich)
                * 1.6
                / NULLIF((sg.so_ghe - 2), 0)
            ) / 10000
        ) * 10000 AS gia_ve
    FROM ve v
    JOIN chuyen_xe cx ON cx.ma_chuyen = v.ma_chuyen
    JOIN cp ON cp.ma_chuyen = cx.ma_chuyen
    JOIN so_ghe sg ON sg.ma_xe = cx.ma_xe
    JOIN ghe g ON g.ma_ghe = v.ma_ghe
             AND g.ma_xe_bus = cx.ma_xe
    LEFT JOIN le_phi lp ON lp.ma_chuyen = cx.ma_chuyen
    WHERE v.ma_ghe IS NOT NULL;
END //

DELIMITER ;
