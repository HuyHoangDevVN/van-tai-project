-- =============================================================================
-- proc_lay_xe_san_sang - L?y danh sách xe s?n sàng ho?t ð?ng
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.GET_AVAILABLE
-- Parameters: p_ngay (DATE) - Ngày c?n ki?m tra
-- Returns: Danh sách xe có th? ho?t ð?ng trong ngày
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_xe_san_sang //

CREATE PROCEDURE proc_lay_xe_san_sang(
    IN p_ngay DATE
)
BEGIN
    SELECT 
        x.ma_xe,
        x.ten_xe,
        x.bien_so,
        x.hang_san_xuat,
        x.trang_thai,
        x.ma_loai_xe,
        lx.ten_loai_xe,
        lx.so_cho
    FROM xe x
    LEFT JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE x.trang_thai = 'Ho?t ð?ng'
        -- Không có chuy?n xe trong ngày ðó
        AND x.ma_xe NOT IN (
            SELECT DISTINCT ma_xe 
            FROM chuyen_xe 
            WHERE DATE(ngay_khoi_hanh) = COALESCE(p_ngay, CURDATE())
                AND trang_thai IN ('Scheduled', 'In Progress')
        )
        -- Không ðang b?o tr?
        AND x.ma_xe NOT IN (
            SELECT DISTINCT ma_xe 
            FROM bao_tri 
            WHERE DATE(ngay_bao_tri) = COALESCE(p_ngay, CURDATE())
        )
    ORDER BY x.ten_xe;
END //

DELIMITER ;
