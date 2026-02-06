-- =============================================================================
-- proc_lay_chuyen_xe_theo_trang_thai - L?y chuy?n xe theo tr?ng thái
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.GET_BY_STATUS
-- Parameters: p_trang_thai (VARCHAR)
-- Returns: Danh sách chuy?n xe theo tr?ng thái
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_chuyen_xe_theo_trang_thai //

CREATE PROCEDURE proc_lay_chuyen_xe_theo_trang_thai(
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        cx.ngay_khoi_hanh,
        cx.trang_thai,
        (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen) AS so_ve_da_ban
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE cx.trang_thai = p_trang_thai
    ORDER BY cx.ngay_khoi_hanh DESC;
END //

DELIMITER ;
