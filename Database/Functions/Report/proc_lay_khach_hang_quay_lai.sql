-- =============================================================================
-- proc_lay_khach_hang_quay_lai - L?y danh sách khách hàng quay l?i
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.KHACH_HANG_QUAY_LAI
-- Parameters: Không có
-- Returns: ma_khach_hang, ten_khach_hang, so_dien_thoai, so_lan_dat_ve, tong_chi_tieu
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_khach_hang_quay_lai //

CREATE PROCEDURE proc_lay_khach_hang_quay_lai()
BEGIN
    SELECT 
        kh.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        COUNT(v.ma_ve) AS so_lan_dat_ve,
        COALESCE(SUM(g.gia), 0) AS tong_chi_tieu,
        MIN(cx.ngay_khoi_hanh) AS lan_dau_dat_ve,
        MAX(cx.ngay_khoi_hanh) AS lan_cuoi_dat_ve
    FROM khach_hang kh
    INNER JOIN ve v ON kh.ma_khach_hang = v.ma_khach_hang
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN gia g ON cx.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    GROUP BY kh.ma_khach_hang, kh.ten_khach_hang, kh.so_dien_thoai, kh.email
    HAVING COUNT(v.ma_ve) > 1 -- Ch? l?y khách hàng ð?t vé >= 2 l?n
    ORDER BY so_lan_dat_ve DESC, tong_chi_tieu DESC;
END //

DELIMITER ;
