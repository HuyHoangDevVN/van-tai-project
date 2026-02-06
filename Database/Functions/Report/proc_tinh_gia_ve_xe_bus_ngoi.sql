-- =============================================================================
-- proc_tinh_gia_ve_xe_bus_ngoi - L?y giá vé xe bus ng?i theo tuy?n
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.GIA_VE_XE_BUS_NGOI
-- Parameters: Không có
-- Returns: ma_tuyen, ten_tuyen, diem_di, diem_den, khoang_cach, gia_ve
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_gia_ve_xe_bus_ngoi //

CREATE PROCEDURE proc_tinh_gia_ve_xe_bus_ngoi()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        g.gia AS gia_ve
    FROM tuyen_duong td
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen
    WHERE g.ma_loai_xe = 'BUS_NGOI'
    ORDER BY td.ten_tuyen;
END //

DELIMITER ;
