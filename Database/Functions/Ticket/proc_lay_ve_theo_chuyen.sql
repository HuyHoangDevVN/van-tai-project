-- =============================================================================
-- proc_lay_ve_theo_chuyen - L?y danh sách vé theo chuy?n xe
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.GET_BY_TRIP
-- Parameters: p_ma_chuyen (VARCHAR)
-- Returns: Danh sách vé c?a chuy?n xe
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_ve_theo_chuyen //

CREATE PROCEDURE proc_lay_ve_theo_chuyen(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai
    FROM ve v
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE v.ma_chuyen = p_ma_chuyen
    ORDER BY v.so_ghe;
END //

DELIMITER ;
