-- =============================================================================
-- proc_tim_kiem_xe - Tìm kiếm xe với phân trang
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.SEARCH
-- Parameters: p_keyword, p_status, p_offset, p_limit, p_sort_by, p_sort_desc, p_total_record (OUT)
-- Returns: Danh sách xe với thông tin chi tiết
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tim_kiem_xe //

CREATE PROCEDURE proc_tim_kiem_xe(
    IN p_keyword VARCHAR(200),
    IN p_status VARCHAR(30),
    IN p_hang_san_xuat VARCHAR(50),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    -- Đếm tổng số bản ghi
    SELECT COUNT(*) INTO p_total_record
    FROM xe x
    WHERE 
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_status IS NULL OR x.trang_thai = p_status)
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat);

    -- Trả về dữ liệu phân trang
    SELECT 
        x.ma_xe,
        x.ten_xe,
        x.bien_so,
        x.hang_san_xuat,
        x.nam_san_xuat,
        x.ngay_dang_kiem,
        x.trang_thai,
        x.muc_tieu_hao,
        x.phu_thu_phi_van_hanh,
        x.ma_loai_xe
    FROM xe x
    WHERE 
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_status IS NULL OR x.trang_thai = p_status)
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat)
    ORDER BY
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 1 THEN x.bien_so END DESC,
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 0 THEN x.bien_so END ASC,
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 1 THEN x.ten_xe END DESC,
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 0 THEN x.ten_xe END ASC,
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 1 THEN x.nam_san_xuat END DESC,
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 0 THEN x.nam_san_xuat END ASC,
        x.ma_xe DESC
    LIMIT p_limit OFFSET p_offset;
END //

DELIMITER ;
