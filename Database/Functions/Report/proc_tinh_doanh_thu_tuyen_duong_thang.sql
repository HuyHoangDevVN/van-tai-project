-- =============================================================================
-- proc_tinh_doanh_thu_tuyen_duong_thang - Tính doanh thu tuy?n ðý?ng theo tháng
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.DOANH_THU_TUYEN_DUONG_THANG
-- Parameters: p_tu (DATETIME), p_den (DATETIME)
-- Returns: ma_tuyen, ten_tuyen, thang, nam, so_chuyen, tong_doanh_thu
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_tuyen_duong_thang //

CREATE PROCEDURE proc_tinh_doanh_thu_tuyen_duong_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        MONTH(cx.ngay_khoi_hanh) AS thang,
        YEAR(cx.ngay_khoi_hanh) AS nam,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM tuyen_duong td
    INNER JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE cx.ngay_khoi_hanh >= p_tu
        AND cx.ngay_khoi_hanh <= p_den
    GROUP BY td.ma_tuyen, td.ten_tuyen, YEAR(cx.ngay_khoi_hanh), MONTH(cx.ngay_khoi_hanh)
    ORDER BY nam, thang, tong_doanh_thu DESC;
END //

DELIMITER ;
