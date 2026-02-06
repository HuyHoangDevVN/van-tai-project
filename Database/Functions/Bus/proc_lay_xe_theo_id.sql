-- =============================================================================
-- proc_lay_xe_theo_id - L?y thông tin xe theo ID
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.GET_BY_ID
-- Parameters: p_ma_xe (VARCHAR)
-- Returns: Thông tin chi ti?t xe
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_lay_xe_theo_id //

CREATE PROCEDURE proc_lay_xe_theo_id(
    IN p_ma_xe VARCHAR(20)
)
BEGIN
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
        x.ma_loai_xe,
        lx.ten_loai_xe,
        lx.so_cho
    FROM xe x
    LEFT JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE x.ma_xe = p_ma_xe;
END //

DELIMITER ;
