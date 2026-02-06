-- =============================================================================
-- proc_tinh_doanh_thu_tuyen_duong - Tính doanh thu theo tuy?n ðý?ng
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.DOANH_THU_TUYEN_DUONG
-- Parameters: Không có
-- Returns: ma_tuyen, ten_tuyen, diem_di, diem_den, so_chuyen, so_ve, tong_doanh_thu
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_tuyen_duong //

CREATE PROCEDURE proc_tinh_doanh_thu_tuyen_duong()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COUNT(v.ma_ve) AS so_ve,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM tuyen_duong td
    LEFT JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    LEFT JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    LEFT JOIN xe x ON cx.ma_xe = x.ma_xe
    LEFT JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    GROUP BY td.ma_tuyen, td.ten_tuyen, td.diem_di, td.diem_den, td.khoang_cach
    ORDER BY tong_doanh_thu DESC;
END //

DELIMITER ;
