-- =============================================================================
-- proc_lay_ve_theo_id - L?y thông tin vé theo ID
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.GET_BY_ID
-- Parameters: p_ma_ve (VARCHAR)
-- Returns: Thông tin chi ti?t vé
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_ve_theo_id //

CREATE PROCEDURE proc_lay_ve_theo_id(
    IN p_ma_ve VARCHAR(20)
)
BEGIN
    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai,
        cx.ngay_khoi_hanh,
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        x.bien_so,
        x.ten_xe,
        g.gia AS gia_ve
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    LEFT JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE v.ma_ve = p_ma_ve;
END //

DELIMITER ;
