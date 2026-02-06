-- =============================================================================
-- proc_cap_nhat_tuyen_duong - C?p nh?t tuy?n ðý?ng
-- =============================================================================
-- Module: ROUTE
-- Function Key: FunctionKeys.Route.UPDATE
-- Parameters: p_ma_tuyen, p_ten_tuyen, p_diem_di, p_diem_den, p_khoang_cach, p_ma_do_phuc_tap
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_cap_nhat_tuyen_duong //

CREATE PROCEDURE proc_cap_nhat_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20),
    IN p_ten_tuyen VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_khoang_cach DECIMAL(10,2),
    IN p_ma_do_phuc_tap VARCHAR(20)
)
BEGIN
    -- Ki?m tra tuy?n ðý?ng t?n t?i
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuy?n ðý?ng không t?n t?i';
    END IF;

    -- C?p nh?t
    UPDATE tuyen_duong
    SET 
        ten_tuyen = COALESCE(p_ten_tuyen, ten_tuyen),
        diem_di = COALESCE(p_diem_di, diem_di),
        diem_den = COALESCE(p_diem_den, diem_den),
        khoang_cach = COALESCE(p_khoang_cach, khoang_cach),
        ma_do_phuc_tap = COALESCE(p_ma_do_phuc_tap, ma_do_phuc_tap)
    WHERE ma_tuyen = p_ma_tuyen;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
