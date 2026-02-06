-- =============================================================================
-- proc_dat_ve - Ð?t vé xe
-- =============================================================================
-- Module: TICKET
-- Function Key: FunctionKeys.Ticket.BOOK
-- Parameters: p_ma_ve, p_ma_chuyen, p_ma_khach_hang, p_so_ghe
-- Returns: S? d?ng b? ?nh hý?ng
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_dat_ve //

CREATE PROCEDURE proc_dat_ve(
    IN p_ma_ve VARCHAR(20),
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_khach_hang VARCHAR(20),
    IN p_so_ghe VARCHAR(10)
)
BEGIN
    DECLARE v_so_cho INT;
    DECLARE v_so_ve_da_ban INT;

    -- Ki?m tra chuy?n xe t?n t?i và ðang ho?t ð?ng
    IF NOT EXISTS (
        SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen AND trang_thai = 'Scheduled'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuy?n xe không t?n t?i ho?c không c?n nh?n ð?t vé';
    END IF;

    -- Ki?m tra khách hàng t?n t?i
    IF NOT EXISTS (SELECT 1 FROM khach_hang WHERE ma_khach_hang = p_ma_khach_hang) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khách hàng không t?n t?i';
    END IF;

    -- Ki?m tra gh? ð? ðý?c ð?t chýa
    IF EXISTS (
        SELECT 1 FROM ve WHERE ma_chuyen = p_ma_chuyen AND so_ghe = p_so_ghe AND trang_thai != 'Cancelled'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Gh? này ð? ðý?c ð?t';
    END IF;

    -- Ki?m tra c?n ch? tr?ng
    SELECT lx.so_cho INTO v_so_cho
    FROM chuyen_xe cx
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE cx.ma_chuyen = p_ma_chuyen;

    SELECT COUNT(*) INTO v_so_ve_da_ban
    FROM ve 
    WHERE ma_chuyen = p_ma_chuyen AND trang_thai != 'Cancelled';

    IF v_so_ve_da_ban >= v_so_cho THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuy?n xe ð? h?t ch?';
    END IF;

    -- Ð?t vé
    INSERT INTO ve (ma_ve, ma_chuyen, ma_khach_hang, so_ghe, ngay_dat, trang_thai)
    VALUES (p_ma_ve, p_ma_chuyen, p_ma_khach_hang, p_so_ghe, NOW(), 'Booked');

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
