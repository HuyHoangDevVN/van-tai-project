-- =============================================================================
-- proc_lay_tuyen_duong_xe_giuong_nam - L?y tuy?n ðý?ng có xe giý?ng n?m
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.TUYEN_DUONG_XE_GIUONG_NAM
-- Parameters: Không có
-- Returns: ma_tuyen, ten_tuyen, diem_di, diem_den, khoang_cach, gia_ve, so_chuyen
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_tuyen_duong_xe_giuong_nam //

CREATE PROCEDURE proc_lay_tuyen_duong_xe_giuong_nam()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        g.gia AS gia_ve,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen
    FROM tuyen_duong td
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen
    LEFT JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    LEFT JOIN xe x ON cx.ma_xe = x.ma_xe AND x.ma_loai_xe = 'GIUONG_NAM'
    WHERE g.ma_loai_xe = 'GIUONG_NAM'
    GROUP BY td.ma_tuyen, td.ten_tuyen, td.diem_di, td.diem_den, td.khoang_cach, g.gia
    ORDER BY td.ten_tuyen;
END //

DELIMITER ;
