-- =============================================================================
-- proc_huy_ve - H?y vé xe
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.CANCEL
-- Parameters: p_ma_ve (VARCHAR)
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_huy_ve //

CREATE PROCEDURE proc_huy_ve(
    IN p_ma_ve VARCHAR(20)
)
BEGIN
    DECLARE v_trang_thai VARCHAR(30);
    DECLARE v_ngay_khoi_hanh DATETIME;

    -- Ki?m tra vé t?n t?i
    SELECT v.trang_thai, cx.ngay_khoi_hanh 
    INTO v_trang_thai, v_ngay_khoi_hanh
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    WHERE v.ma_ve = p_ma_ve;

    IF v_trang_thai IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Vé không t?n t?i';
    END IF;

    -- Ki?m tra vé chýa b? h?y
    IF v_trang_thai = 'Cancelled' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Vé ð? ðý?c h?y trý?c ðó';
    END IF;

    -- Ki?m tra chuy?n xe chýa kh?i hành
    IF v_ngay_khoi_hanh <= NOW() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Không th? h?y vé c?a chuy?n xe ð? kh?i hành';
    END IF;

    -- H?y vé
    UPDATE ve
    SET trang_thai = 'Cancelled'
    WHERE ma_ve = p_ma_ve;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
