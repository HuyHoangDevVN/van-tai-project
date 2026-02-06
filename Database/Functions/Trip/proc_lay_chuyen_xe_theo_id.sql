-- =============================================================================
-- proc_lay_chuyen_xe_theo_id - L?y thông tin chuy?n xe theo ID
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.GET_BY_ID
-- Parameters: p_ma_chuyen (VARCHAR)
-- Returns: Thông tin chi ti?t chuy?n xe
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_chuyen_xe_theo_id //

CREATE PROCEDURE proc_lay_chuyen_xe_theo_id(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        x.ma_loai_xe,
        lx.ten_loai_xe,
        lx.so_cho,
        cx.ngay_khoi_hanh,
        cx.trang_thai,
        -- S? vé ð? bán
        (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen) AS so_ve_da_ban,
        -- S? gh? c?n tr?ng
        (lx.so_cho - (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen)) AS so_ghe_trong
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    LEFT JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE cx.ma_chuyen = p_ma_chuyen;
END //

DELIMITER ;
