-- =============================================================================
-- proc_cap_nhat_xe - C?p nh?t thông tin xe
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.UPDATE
-- Parameters: p_ma_xe, p_ten_xe, p_bien_so, p_hang_san_xuat, p_nam_san_xuat, 
--             p_ngay_dang_kiem, p_trang_thai, p_muc_tieu_hao, p_phu_thu_phi_van_hanh, p_ma_loai_xe
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_xe //

CREATE PROCEDURE proc_cap_nhat_xe(
    IN p_ma_xe VARCHAR(20),
    IN p_ten_xe VARCHAR(100),
    IN p_bien_so VARCHAR(20),
    IN p_hang_san_xuat VARCHAR(50),
    IN p_nam_san_xuat INT,
    IN p_ngay_dang_kiem DATE,
    IN p_trang_thai VARCHAR(30),
    IN p_muc_tieu_hao DECIMAL(5,2),
    IN p_phu_thu_phi_van_hanh DECIMAL(15,2),
    IN p_ma_loai_xe VARCHAR(20)
)
BEGIN
    -- Ki?m tra xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe không t?n t?i';
    END IF;

    -- Ki?m tra bi?n s? không trùng v?i xe khác
    IF EXISTS (SELECT 1 FROM xe WHERE bien_so = p_bien_so AND ma_xe != p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bi?n s? xe ð? ðý?c s? d?ng b?i xe khác';
    END IF;

    -- C?p nh?t
    UPDATE xe
    SET 
        ten_xe = COALESCE(p_ten_xe, ten_xe),
        bien_so = COALESCE(p_bien_so, bien_so),
        hang_san_xuat = COALESCE(p_hang_san_xuat, hang_san_xuat),
        nam_san_xuat = COALESCE(p_nam_san_xuat, nam_san_xuat),
        ngay_dang_kiem = COALESCE(p_ngay_dang_kiem, ngay_dang_kiem),
        trang_thai = COALESCE(p_trang_thai, trang_thai),
        muc_tieu_hao = COALESCE(p_muc_tieu_hao, muc_tieu_hao),
        phu_thu_phi_van_hanh = COALESCE(p_phu_thu_phi_van_hanh, phu_thu_phi_van_hanh),
        ma_loai_xe = COALESCE(p_ma_loai_xe, ma_loai_xe)
    WHERE ma_xe = p_ma_xe;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
