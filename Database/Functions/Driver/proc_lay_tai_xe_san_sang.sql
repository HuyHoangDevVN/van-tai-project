-- =============================================================================
-- proc_lay_tai_xe_san_sang - L?y danh sách tài x? s?n sàng
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.GET_AVAILABLE
-- Parameters: p_ngay (DATE)
-- Returns: Danh sách tài x? có th? làm vi?c trong ngày
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_tai_xe_san_sang //

CREATE PROCEDURE proc_lay_tai_xe_san_sang(
    IN p_ngay DATE
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.gioi_tinh,
        tx.so_cccd,
        tx.he_so_luong
    FROM tai_xe tx
    WHERE tx.ma_tai_xe NOT IN (
        -- Tài x? ð? ðý?c phân công xe có chuy?n trong ngày
        SELECT DISTINCT pctx.ma_tai_xe 
        FROM phan_cong_tai_xe pctx
        INNER JOIN chuyen_xe cx ON pctx.ma_xe = cx.ma_xe
        WHERE DATE(cx.ngay_khoi_hanh) = COALESCE(p_ngay, CURDATE())
            AND cx.trang_thai IN ('Scheduled', 'In Progress')
    )
    ORDER BY tx.ten_tai_xe;
END //

DELIMITER ;
