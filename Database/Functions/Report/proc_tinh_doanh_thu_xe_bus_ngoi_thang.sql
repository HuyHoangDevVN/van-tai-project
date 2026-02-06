-- =============================================================================
-- proc_tinh_doanh_thu_xe_bus_ngoi_thang - Tính doanh thu xe bus ng?i theo tháng
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.DOANH_THU_XE_BUS_NGOI_THANG
-- Parameters: p_tu (DATETIME), p_den (DATETIME)
-- Returns: ma_xe, bien_so, ten_xe, thang, nam, so_chuyen, tong_doanh_thu
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_xe_bus_ngoi_thang //

CREATE PROCEDURE proc_tinh_doanh_thu_xe_bus_ngoi_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        MONTH(cx.ngay_khoi_hanh) AS thang,
        YEAR(cx.ngay_khoi_hanh) AS nam,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM xe x
    INNER JOIN chuyen_xe cx ON x.ma_xe = cx.ma_xe
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN gia g ON cx.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE x.ma_loai_xe = 'BUS_NGOI'
        AND cx.ngay_khoi_hanh >= p_tu
        AND cx.ngay_khoi_hanh <= p_den
    GROUP BY x.ma_xe, x.bien_so, x.ten_xe, YEAR(cx.ngay_khoi_hanh), MONTH(cx.ngay_khoi_hanh)
    ORDER BY nam, thang, tong_doanh_thu DESC;
END //

DELIMITER ;
