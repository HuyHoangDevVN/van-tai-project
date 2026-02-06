-- =============================================================================
-- proc_tim_kiem_tuyen_duong - T?m ki?m tuy?n ðý?ng v?i phân trang
-- =============================================================================
-- Module: ROUTE
-- Function Key: FunctionKeys.Route.SEARCH
-- Parameters: p_keyword, p_diem_di, p_diem_den, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách tuy?n ðý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_tuyen_duong //

CREATE PROCEDURE proc_tim_kiem_tuyen_duong(
    IN p_keyword VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Ð?m t?ng s? b?n ghi
    SELECT COUNT(*) INTO p_total_record
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'));

    -- Tr? v? d? li?u phân trang
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 1 THEN td.ten_tuyen END DESC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 0 THEN td.ten_tuyen END ASC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 1 THEN td.khoang_cach END DESC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 0 THEN td.khoang_cach END ASC,
        td.ma_tuyen DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
