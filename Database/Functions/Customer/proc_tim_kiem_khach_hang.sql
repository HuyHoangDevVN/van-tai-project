-- =============================================================================
-- proc_tim_kiem_khach_hang - T?m ki?m khách hàng v?i phân trang
-- =============================================================================
-- Module: CUSTOMER
-- Function Key: FunctionKeys.Customer.SEARCH
-- Parameters: p_keyword, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách khách hàng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_khach_hang //

CREATE PROCEDURE proc_tim_kiem_khach_hang(
    IN p_keyword VARCHAR(200),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Ð?m t?ng s? b?n ghi
    SELECT COUNT(*) INTO p_total_record
    FROM khach_hang kh
    WHERE 
        (p_keyword IS NULL OR (
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.ma_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%') OR
            kh.email LIKE CONCAT('%', p_keyword, '%')
        ));

    -- Tr? v? d? li?u phân trang
    SELECT 
        kh.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        kh.dia_chi
    FROM khach_hang kh
    WHERE 
        (p_keyword IS NULL OR (
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.ma_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%') OR
            kh.email LIKE CONCAT('%', p_keyword, '%')
        ))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 1 THEN kh.ten_khach_hang END DESC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 0 THEN kh.ten_khach_hang END ASC,
        kh.ma_khach_hang DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
