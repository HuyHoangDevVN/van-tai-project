-- =============================================================================
-- proc_lay_khach_hang_theo_id - L?y thông tin khách hàng theo ID
-- =============================================================================
-- Module: CUSTOMER
-- Function Key: FunctionKeys.Customer.GET_BY_ID
-- Parameters: p_ma_khach_hang (VARCHAR)
-- Returns: Thông tin chi ti?t khách hàng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_khach_hang_theo_id //

CREATE PROCEDURE proc_lay_khach_hang_theo_id(
    IN p_ma_khach_hang VARCHAR(20)
)
BEGIN
    SELECT 
        kh.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        kh.dia_chi,
        -- Th?ng kê
        (SELECT COUNT(*) FROM ve WHERE ma_khach_hang = kh.ma_khach_hang) AS tong_ve_da_dat,
        (SELECT COUNT(*) FROM ve WHERE ma_khach_hang = kh.ma_khach_hang AND trang_thai = 'Completed') AS tong_ve_hoan_thanh
    FROM khach_hang kh
    WHERE kh.ma_khach_hang = p_ma_khach_hang;
END //

DELIMITER ;
