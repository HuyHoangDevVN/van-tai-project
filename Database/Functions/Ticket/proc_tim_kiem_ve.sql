-- =============================================================================
-- proc_tim_kiem_ve - T?m ki?m vé v?i phân trang
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.SEARCH
-- Parameters: p_keyword, p_trang_thai, p_ma_chuyen, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách vé
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_ve //

CREATE PROCEDURE proc_tim_kiem_ve(
    IN p_keyword VARCHAR(200),
    IN p_trang_thai VARCHAR(30),
    IN p_ma_chuyen VARCHAR(20),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Ð?m t?ng s? b?n ghi
    SELECT COUNT(*) INTO p_total_record
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE 
        (p_keyword IS NULL OR (
            v.ma_ve LIKE CONCAT('%', p_keyword, '%') OR
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR v.trang_thai = p_trang_thai)
        AND (p_ma_chuyen IS NULL OR v.ma_chuyen = p_ma_chuyen);

    -- Tr? v? d? li?u phân trang
    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai,
        cx.ngay_khoi_hanh,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE 
        (p_keyword IS NULL OR (
            v.ma_ve LIKE CONCAT('%', p_keyword, '%') OR
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR v.trang_thai = p_trang_thai)
        AND (p_ma_chuyen IS NULL OR v.ma_chuyen = p_ma_chuyen)
    ORDER BY
        CASE WHEN p_sort_by = 'ngay_dat' AND p_sort_desc = 1 THEN v.ngay_dat END DESC,
        CASE WHEN p_sort_by = 'ngay_dat' AND p_sort_desc = 0 THEN v.ngay_dat END ASC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 1 THEN kh.ten_khach_hang END DESC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 0 THEN kh.ten_khach_hang END ASC,
        v.ma_ve DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
