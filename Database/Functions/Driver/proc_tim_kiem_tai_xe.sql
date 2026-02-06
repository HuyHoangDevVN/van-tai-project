-- =============================================================================
-- proc_tim_kiem_tai_xe - T?m ki?m tài x? v?i phân trang
-- =============================================================================
-- Module: DRIVER
-- Function Key: FunctionKeys.Driver.SEARCH
-- Parameters: p_keyword, p_gioi_tinh, p_que_quan, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách tài x?
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_tai_xe //

CREATE PROCEDURE proc_tim_kiem_tai_xe(
    IN p_keyword VARCHAR(200),
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Ð?m t?ng s? b?n ghi
    SELECT COUNT(*) INTO p_total_record
    FROM tai_xe tx
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'));

    -- Tr? v? d? li?u phân trang
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
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 1 THEN tx.ten_tai_xe END DESC,
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 0 THEN tx.ten_tai_xe END ASC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 1 THEN tx.ngay_ky_hop_dong END DESC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 0 THEN tx.ngay_ky_hop_dong END ASC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 1 THEN tx.he_so_luong END DESC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 0 THEN tx.he_so_luong END ASC,
        tx.ma_tai_xe DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
