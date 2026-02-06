-- =============================================================================
-- proc_tim_kiem_chuyen_xe - T?m ki?m chuy?n xe v?i phân trang
-- =============================================================================
-- Module: TRIP
-- Function Key: FunctionKeys.Trip.SEARCH
-- Parameters: p_keyword, p_trang_thai, p_ma_tuyen, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách chuy?n xe
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_chuyen_xe //

CREATE PROCEDURE proc_tim_kiem_chuyen_xe(
    IN p_keyword VARCHAR(200),
    IN p_trang_thai VARCHAR(30),
    IN p_ma_tuyen VARCHAR(20),
    IN p_tu_ngay DATETIME,
    IN p_den_ngay DATETIME,
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Ð?m t?ng s? b?n ghi
    SELECT COUNT(*) INTO p_total_record
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE 
        (p_keyword IS NULL OR (
            cx.ma_chuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR cx.trang_thai = p_trang_thai)
        AND (p_ma_tuyen IS NULL OR cx.ma_tuyen = p_ma_tuyen)
        AND (p_tu_ngay IS NULL OR cx.ngay_khoi_hanh >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR cx.ngay_khoi_hanh <= p_den_ngay);

    -- Tr? v? d? li?u phân trang
    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        cx.ngay_khoi_hanh,
        cx.trang_thai
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE 
        (p_keyword IS NULL OR (
            cx.ma_chuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR cx.trang_thai = p_trang_thai)
        AND (p_ma_tuyen IS NULL OR cx.ma_tuyen = p_ma_tuyen)
        AND (p_tu_ngay IS NULL OR cx.ngay_khoi_hanh >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR cx.ngay_khoi_hanh <= p_den_ngay)
    ORDER BY
        CASE WHEN p_sort_by = 'ngay_khoi_hanh' AND p_sort_desc = 1 THEN cx.ngay_khoi_hanh END DESC,
        CASE WHEN p_sort_by = 'ngay_khoi_hanh' AND p_sort_desc = 0 THEN cx.ngay_khoi_hanh END ASC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 1 THEN td.ten_tuyen END DESC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 0 THEN td.ten_tuyen END ASC,
        cx.ma_chuyen DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
