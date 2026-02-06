-- =============================================================================
-- proc_lay_tuyen_duong_theo_id - L?y thông tin tuy?n ðý?ng theo ID
-- =============================================================================
-- Module: ROUTE
-- Function Key: FunctionKeys.Route.GET_BY_ID
-- Parameters: p_ma_tuyen (VARCHAR)
-- Returns: Thông tin chi ti?t tuy?n ðý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_tuyen_duong_theo_id //

CREATE PROCEDURE proc_lay_tuyen_duong_theo_id(
    IN p_ma_tuyen VARCHAR(20)
)
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap
    FROM tuyen_duong td
    WHERE td.ma_tuyen = p_ma_tuyen;
END //

DELIMITER ;
