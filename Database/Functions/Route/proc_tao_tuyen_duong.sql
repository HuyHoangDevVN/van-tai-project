-- =============================================================================
-- proc_tao_tuyen_duong - T?o m?i tuy?n ðý?ng
-- =============================================================================
-- Module: ROUTE
-- Function Key: FunctionKeys.Route.CREATE
-- Parameters: p_ma_tuyen, p_ten_tuyen, p_diem_di, p_diem_den, p_khoang_cach, p_ma_do_phuc_tap
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tao_tuyen_duong //

CREATE PROCEDURE proc_tao_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20),
    IN p_ten_tuyen VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_khoang_cach DECIMAL(10,2),
    IN p_ma_do_phuc_tap VARCHAR(20)
)
BEGIN
    -- Ki?m tra tuy?n ðý?ng ð? t?n t?i (cùng ði?m ði và ði?m ð?n)
    IF EXISTS (SELECT 1 FROM tuyen_duong WHERE diem_di = p_diem_di AND diem_den = p_diem_den) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuy?n ðý?ng v?i ði?m ði và ði?m ð?n này ð? t?n t?i';
    END IF;

    -- Thêm tuy?n ðý?ng m?i
    INSERT INTO tuyen_duong (
        ma_tuyen, ten_tuyen, diem_di, diem_den, khoang_cach, ma_do_phuc_tap
    )
    VALUES (
        p_ma_tuyen, p_ten_tuyen, p_diem_di, p_diem_den, p_khoang_cach, p_ma_do_phuc_tap
    );

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
