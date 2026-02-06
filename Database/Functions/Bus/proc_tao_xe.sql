-- =============================================================================
-- proc_tao_xe - T?o m?i xe
-- =============================================================================
-- Module: BUS
-- Function Key: FunctionKeys.Bus.CREATE
-- Parameters: p_ma_xe, p_ten_xe, p_bien_so, p_hang_san_xuat, p_nam_san_xuat, 
--             p_ngay_dang_kiem, p_trang_thai, p_muc_tieu_hao, p_phu_thu_phi_van_hanh, p_ma_loai_xe
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_xe //

CREATE PROCEDURE proc_tao_xe(
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
    -- Ki?m tra bi?n s? ð? t?n t?i
    IF EXISTS (SELECT 1 FROM xe WHERE bien_so = p_bien_so) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bi?n s? xe ð? t?n t?i';
    END IF;

    -- Ki?m tra lo?i xe t?n t?i
    IF NOT EXISTS (SELECT 1 FROM loai_xe WHERE ma_loai_xe = p_ma_loai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Lo?i xe không t?n t?i';
    END IF;

    -- Thêm xe m?i
    INSERT INTO xe (
        ma_xe, ten_xe, bien_so, hang_san_xuat, nam_san_xuat,
        ngay_dang_kiem, trang_thai, muc_tieu_hao, phu_thu_phi_van_hanh, ma_loai_xe
    )
    VALUES (
        p_ma_xe, p_ten_xe, p_bien_so, p_hang_san_xuat, p_nam_san_xuat,
        p_ngay_dang_kiem, COALESCE(p_trang_thai, 'Ho?t ð?ng'), p_muc_tieu_hao, 
        p_phu_thu_phi_van_hanh, p_ma_loai_xe
    );

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
