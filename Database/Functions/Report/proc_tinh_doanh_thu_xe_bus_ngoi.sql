-- =============================================================================
-- proc_tinh_doanh_thu_xe_bus_ngoi - Tính doanh thu xe bus ng?i
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.DOANH_THU_XE_BUS_NGOI
-- Parameters: Không có
-- Returns: ma_xe, bien_so, ten_xe, so_chuyen, tong_doanh_thu
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_xe_bus_ngoi //

CREATE PROCEDURE proc_tinh_doanh_thu_xe_bus_ngoi()
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM xe x
    INNER JOIN chuyen_xe cx ON x.ma_xe = cx.ma_xe
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN gia g ON cx.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE x.ma_loai_xe = 'BUS_NGOI' -- M? lo?i xe bus ng?i
    GROUP BY x.ma_xe, x.bien_so, x.ten_xe
    ORDER BY tong_doanh_thu DESC;
END //

DELIMITER ;
