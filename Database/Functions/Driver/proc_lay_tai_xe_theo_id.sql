-- =============================================================================
-- proc_lay_tai_xe_theo_id - L?y thông tin tài x? theo ID
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.GET_BY_ID
-- Parameters: p_ma_tai_xe (VARCHAR)
-- Returns: Thông tin chi ti?t tài x?
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_tai_xe_theo_id //

CREATE PROCEDURE proc_lay_tai_xe_theo_id(
    IN p_ma_tai_xe VARCHAR(20)
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.ngay_sinh,
        tx.gioi_tinh,
        tx.que_quan,
        tx.ton_giao,
        tx.so_cccd,
        tx.ngay_ky_hop_dong,
        tx.tuoi,
        tx.he_so_luong
    FROM tai_xe tx
    WHERE tx.ma_tai_xe = p_ma_tai_xe;
END //

DELIMITER ;
