-- =============================================================================
-- File: 003_Trip_Ticket_StoredProcedures.sql
-- Description: Stored Procedures for Trip Management, Ticketing & Maintenance
-- Database: quan_ly_hang_van_tai_hanh_khach_duong_dai
-- =============================================================================

USE quan_ly_hang_van_tai_hanh_khach_duong_dai;

-- =============================================================================
-- Add required columns to xe table for maintenance tracking
-- =============================================================================

ALTER TABLE xe 
    ADD COLUMN IF NOT EXISTS tong_km_van_hanh FLOAT DEFAULT 0 COMMENT 'Tổng km vận hành (cho bảo trì)',
    ADD COLUMN IF NOT EXISTS ngay_bao_tri_cuoi DATE NULL COMMENT 'Ngày bảo trì gần nhất',
    ADD COLUMN IF NOT EXISTS so_cho_ngoi INT DEFAULT 45 COMMENT 'Số chỗ ngồi';

-- Add column to tai_xe for trip counting
ALTER TABLE tai_xe
    ADD COLUMN IF NOT EXISTS tong_so_chuyen INT DEFAULT 0 COMMENT 'Tổng số chuyến đã chạy';

-- Add status column to chuyen_xe
ALTER TABLE chuyen_xe
    ADD COLUMN IF NOT EXISTS trang_thai VARCHAR(30) DEFAULT 'Scheduled' COMMENT 'Trạng thái: Scheduled, InProgress, Completed, Cancelled';

-- =============================================================================
-- MAINTENANCE MODULE: sp_maintenance_status_check
-- Kiểm tra trạng thái bảo trì (360 ngày hoặc km vận hành)
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_maintenance_status_check;

DELIMITER //
CREATE PROCEDURE sp_maintenance_status_check()
BEGIN
    -- Lấy danh sách xe cần bảo trì
    -- Điều kiện: 
    --   1. Quá 360 ngày kể từ lần bảo trì cuối
    --   2. Km vận hành vượt ngưỡng (giả sử 50000 km)
    
    DECLARE v_maintenance_days INT DEFAULT 360;
    DECLARE v_maintenance_km FLOAT DEFAULT 50000;
    
    SELECT 
        x.ma_xe,
        x.ten_xe,
        x.bien_so,
        x.trang_thai,
        x.tong_km_van_hanh,
        x.ngay_bao_tri_cuoi,
        COALESCE(DATEDIFF(CURDATE(), x.ngay_bao_tri_cuoi), 9999) AS so_ngay_tu_bao_tri,
        CASE 
            WHEN x.ngay_bao_tri_cuoi IS NULL THEN 'Chưa bảo trì lần nào'
            WHEN DATEDIFF(CURDATE(), x.ngay_bao_tri_cuoi) > v_maintenance_days THEN 'Quá hạn bảo trì (ngày)'
            WHEN x.tong_km_van_hanh > v_maintenance_km THEN 'Cần bảo trì (km)'
            ELSE 'Bình thường'
        END AS trang_thai_bao_tri,
        CASE 
            WHEN x.ngay_bao_tri_cuoi IS NULL OR DATEDIFF(CURDATE(), x.ngay_bao_tri_cuoi) > v_maintenance_days 
                OR x.tong_km_van_hanh > v_maintenance_km 
            THEN 1 
            ELSE 0 
        END AS can_bao_tri
    FROM xe x
    ORDER BY can_bao_tri DESC, so_ngay_tu_bao_tri DESC;
END //
DELIMITER ;

-- =============================================================================
-- MAINTENANCE MODULE: sp_maintenance_create
-- Thêm lịch bảo trì mới và reset km vận hành
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_maintenance_create;

DELIMITER //
CREATE PROCEDURE sp_maintenance_create(
    IN p_ma_bao_tri VARCHAR(20),
    IN p_ma_xe VARCHAR(20),
    IN p_don_vi VARCHAR(100),
    IN p_chi_phi DECIMAL(15,2),
    IN p_ngay DATE,
    IN p_so_km INT,
    OUT p_response_code INT,
    OUT p_response_message VARCHAR(500)
)
BEGIN
    DECLARE v_xe_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE, @errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
        SET p_response_code = -1;
        SET p_response_message = CONCAT('Lỗi: ', @text);
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Kiểm tra xe tồn tại
    SELECT COUNT(*) INTO v_xe_exists FROM xe WHERE ma_xe = p_ma_xe;
    
    IF v_xe_exists = 0 THEN
        SET p_response_code = -3;
        SET p_response_message = 'Không tìm thấy xe với mã này';
        ROLLBACK;
    ELSE
        -- Thêm bản ghi bảo trì
        INSERT INTO bao_tri (ma_bao_tri, ma_xe, don_vi, chi_phi, ngay, so_km)
        VALUES (p_ma_bao_tri, p_ma_xe, p_don_vi, p_chi_phi, p_ngay, p_so_km);
        
        -- Cập nhật thông tin xe: reset km và ngày bảo trì
        UPDATE xe 
        SET ngay_bao_tri_cuoi = p_ngay,
            tong_km_van_hanh = 0  -- Reset km sau bảo trì
        WHERE ma_xe = p_ma_xe;
        
        SET p_response_code = 0;
        SET p_response_message = 'Thêm bảo trì thành công';
        COMMIT;
    END IF;
END //
DELIMITER ;

-- =============================================================================
-- TRIP MODULE: sp_trip_create
-- Tạo chuyến xe mới với kiểm tra ràng buộc
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_trip_create;

DELIMITER //
CREATE PROCEDURE sp_trip_create(
    IN p_ma_chuyen VARCHAR(20),
    IN p_ten_chuyen VARCHAR(100),
    IN p_thoi_gian_khoi_hanh DATETIME,
    IN p_thoi_gian_den DATETIME,
    IN p_ma_xe VARCHAR(20),
    IN p_ma_tuyen VARCHAR(20),
    OUT p_response_code INT,
    OUT p_response_message VARCHAR(500)
)
BEGIN
    DECLARE v_xe_status VARCHAR(30);
    DECLARE v_xe_busy INT DEFAULT 0;
    DECLARE v_tuyen_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE, @errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
        SET p_response_code = -1;
        SET p_response_message = CONCAT('Lỗi: ', @text);
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Kiểm tra xe có Active không
    SELECT trang_thai INTO v_xe_status FROM xe WHERE ma_xe = p_ma_xe;
    
    IF v_xe_status IS NULL THEN
        SET p_response_code = -3;
        SET p_response_message = 'Không tìm thấy xe với mã này';
        ROLLBACK;
    ELSEIF v_xe_status != 'Active' AND v_xe_status != 'Hoạt động' THEN
        SET p_response_code = -2;
        SET p_response_message = CONCAT('Xe không hoạt động. Trạng thái hiện tại: ', v_xe_status);
        ROLLBACK;
    ELSE
        -- Kiểm tra xe có đang chạy chuyến khác không (thời gian trùng)
        SELECT COUNT(*) INTO v_xe_busy 
        FROM chuyen_xe 
        WHERE ma_xe = p_ma_xe 
          AND trang_thai IN ('Scheduled', 'InProgress')
          AND (
              (p_thoi_gian_khoi_hanh BETWEEN thoi_gian_khoi_hanh AND thoi_gian_den)
              OR (p_thoi_gian_den BETWEEN thoi_gian_khoi_hanh AND thoi_gian_den)
              OR (thoi_gian_khoi_hanh BETWEEN p_thoi_gian_khoi_hanh AND p_thoi_gian_den)
          );
        
        IF v_xe_busy > 0 THEN
            SET p_response_code = -2;
            SET p_response_message = 'Xe đang có chuyến khác trong khoảng thời gian này';
            ROLLBACK;
        ELSE
            -- Kiểm tra tuyến đường tồn tại
            SELECT COUNT(*) INTO v_tuyen_exists FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen;
            
            IF v_tuyen_exists = 0 THEN
                SET p_response_code = -3;
                SET p_response_message = 'Không tìm thấy tuyến đường với mã này';
                ROLLBACK;
            ELSE
                -- Thêm chuyến xe
                INSERT INTO chuyen_xe (ma_chuyen, ten_chuyen, thoi_gian_khoi_hanh, thoi_gian_den, ma_xe, ma_tuyen, trang_thai)
                VALUES (p_ma_chuyen, p_ten_chuyen, p_thoi_gian_khoi_hanh, p_thoi_gian_den, p_ma_xe, p_ma_tuyen, 'Scheduled');
                
                SET p_response_code = 0;
                SET p_response_message = 'Tạo chuyến xe thành công';
                COMMIT;
            END IF;
        END IF;
    END IF;
END //
DELIMITER ;

-- =============================================================================
-- TRIP MODULE: sp_trip_complete_update_stats
-- Hoàn thành chuyến xe & cập nhật km vận hành cho xe và số chuyến cho tài xế
-- CRITICAL: This updates the maintenance algorithm data
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_trip_complete_update_stats;

DELIMITER //
CREATE PROCEDURE sp_trip_complete_update_stats(
    IN p_ma_chuyen VARCHAR(20),
    OUT p_response_code INT,
    OUT p_response_message VARCHAR(500)
)
BEGIN
    DECLARE v_ma_xe VARCHAR(20);
    DECLARE v_ma_tuyen VARCHAR(20);
    DECLARE v_khoang_cach FLOAT DEFAULT 0;
    DECLARE v_he_so_phuc_tap DECIMAL(5,2) DEFAULT 1;
    DECLARE v_added_km FLOAT DEFAULT 0;
    DECLARE v_trip_status VARCHAR(30);
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE, @errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
        SET p_response_code = -1;
        SET p_response_message = CONCAT('Lỗi hệ thống: ', @text);
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Lấy thông tin chuyến xe
    SELECT c.ma_xe, c.ma_tuyen, c.trang_thai 
    INTO v_ma_xe, v_ma_tuyen, v_trip_status
    FROM chuyen_xe c
    WHERE c.ma_chuyen = p_ma_chuyen;
    
    IF v_ma_xe IS NULL THEN
        SET p_response_code = -3;
        SET p_response_message = 'Không tìm thấy chuyến xe với mã này';
        ROLLBACK;
    ELSEIF v_trip_status = 'Completed' THEN
        SET p_response_code = -2;
        SET p_response_message = 'Chuyến xe này đã được hoàn thành trước đó';
        ROLLBACK;
    ELSEIF v_trip_status = 'Cancelled' THEN
        SET p_response_code = -2;
        SET p_response_message = 'Không thể hoàn thành chuyến xe đã bị hủy';
        ROLLBACK;
    ELSE
        -- Lấy khoảng cách tuyến đường và hệ số phức tạp
        SELECT 
            td.khoang_cach,
            COALESCE(dpt.ty_le_chi_phi_van_hanh_tang_them, 1)
        INTO v_khoang_cach, v_he_so_phuc_tap
        FROM tuyen_duong td
        LEFT JOIN do_phuc_tap dpt ON td.ma_do_phuc_tap = dpt.ma_do_phuc_tap
        WHERE td.ma_tuyen = v_ma_tuyen;
        
        -- Tính km vận hành tăng thêm = Khoảng cách * Hệ số phức tạp
        -- (Tuyến đường phức tạp làm tăng độ hao mòn xe)
        SET v_added_km = COALESCE(v_khoang_cach, 0) * COALESCE(v_he_so_phuc_tap, 1);
        
        -- 1. Cập nhật trạng thái chuyến xe
        UPDATE chuyen_xe 
        SET trang_thai = 'Completed'
        WHERE ma_chuyen = p_ma_chuyen;
        
        -- 2. Cập nhật tổng km vận hành của xe (cho thuật toán bảo trì)
        UPDATE xe 
        SET tong_km_van_hanh = tong_km_van_hanh + v_added_km
        WHERE ma_xe = v_ma_xe;
        
        -- 3. Cập nhật số chuyến của tài xế (cho tính lương)
        UPDATE tai_xe tx
        SET tong_so_chuyen = tong_so_chuyen + 1
        WHERE EXISTS (
            SELECT 1 FROM chi_dinh cd 
            WHERE cd.ma_chuyen = p_ma_chuyen 
            AND cd.ma_tai_xe = tx.ma_tai_xe
        );
        
        SET p_response_code = 0;
        SET p_response_message = CONCAT('Hoàn thành chuyến thành công. Km vận hành tăng: ', ROUND(v_added_km, 2), ' km');
        COMMIT;
    END IF;
END //
DELIMITER ;

-- =============================================================================
-- TICKET MODULE: sp_ticket_booking
-- Đặt vé với kiểm tra số ghế trống
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_ticket_booking;

DELIMITER //
CREATE PROCEDURE sp_ticket_booking(
    IN p_ma_khach VARCHAR(20),
    IN p_ma_chuyen VARCHAR(20),
    IN p_phuong_thuc_tt VARCHAR(50),
    IN p_vi_tri VARCHAR(20),
    IN p_ma_ghe INT,
    IN p_ma_giuong INT,
    OUT p_response_code INT,
    OUT p_response_message VARCHAR(500),
    OUT p_stt_ve INT
)
BEGIN
    DECLARE v_trip_status VARCHAR(30);
    DECLARE v_ma_xe VARCHAR(20);
    DECLARE v_so_cho_ngoi INT DEFAULT 0;
    DECLARE v_so_ve_da_ban INT DEFAULT 0;
    DECLARE v_seat_taken INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE, @errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
        SET p_response_code = -1;
        SET p_response_message = CONCAT('Lỗi hệ thống: ', @text);
        SET p_stt_ve = NULL;
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Lấy thông tin chuyến xe và xe
    SELECT c.trang_thai, c.ma_xe 
    INTO v_trip_status, v_ma_xe
    FROM chuyen_xe c
    WHERE c.ma_chuyen = p_ma_chuyen;
    
    IF v_ma_xe IS NULL THEN
        SET p_response_code = -3;
        SET p_response_message = 'Không tìm thấy chuyến xe với mã này';
        SET p_stt_ve = NULL;
        ROLLBACK;
    ELSEIF v_trip_status NOT IN ('Scheduled', 'InProgress') THEN
        SET p_response_code = -2;
        SET p_response_message = CONCAT('Không thể đặt vé cho chuyến xe ở trạng thái: ', v_trip_status);
        SET p_stt_ve = NULL;
        ROLLBACK;
    ELSE
        -- Lấy số chỗ ngồi của xe
        SELECT COALESCE(so_cho_ngoi, 45) INTO v_so_cho_ngoi FROM xe WHERE ma_xe = v_ma_xe;
        
        -- Đếm số vé đã bán cho chuyến này
        SELECT COUNT(*) INTO v_so_ve_da_ban 
        FROM ve 
        WHERE ma_chuyen = p_ma_chuyen 
          AND trang_thai_tt != 'Cancelled';
        
        -- Kiểm tra còn chỗ không
        IF v_so_ve_da_ban >= v_so_cho_ngoi THEN
            SET p_response_code = -2;
            SET p_response_message = CONCAT('Chuyến xe đã hết chỗ (', v_so_cho_ngoi, '/', v_so_cho_ngoi, ')');
            SET p_stt_ve = NULL;
            ROLLBACK;
        ELSE
            -- Kiểm tra ghế đã được đặt chưa (nếu có chỉ định ghế)
            IF p_ma_ghe IS NOT NULL THEN
                SELECT COUNT(*) INTO v_seat_taken 
                FROM ve 
                WHERE ma_chuyen = p_ma_chuyen 
                  AND ma_ghe = p_ma_ghe
                  AND trang_thai_tt != 'Cancelled';
                
                IF v_seat_taken > 0 THEN
                    SET p_response_code = -2;
                    SET p_response_message = 'Ghế này đã được đặt';
                    SET p_stt_ve = NULL;
                    ROLLBACK;
                END IF;
            END IF;
            
            -- Thêm vé mới
            INSERT INTO ve (ma_khach, ma_chuyen, phuong_thuc_tt, thoi_gian_dat, vi_tri, trang_thai_tt, ma_ghe, ma_giuong)
            VALUES (p_ma_khach, p_ma_chuyen, p_phuong_thuc_tt, NOW(), p_vi_tri, 'Pending', p_ma_ghe, p_ma_giuong);
            
            SET p_stt_ve = LAST_INSERT_ID();
            SET p_response_code = 0;
            SET p_response_message = CONCAT('Đặt vé thành công. Số ghế còn lại: ', v_so_cho_ngoi - v_so_ve_da_ban - 1);
            COMMIT;
        END IF;
    END IF;
END //
DELIMITER ;

-- =============================================================================
-- TICKET MODULE: sp_ticket_cancel
-- Hủy vé
-- =============================================================================

DROP PROCEDURE IF EXISTS sp_ticket_cancel;

DELIMITER //
CREATE PROCEDURE sp_ticket_cancel(
    IN p_stt INT,
    OUT p_response_code INT,
    OUT p_response_message VARCHAR(500)
)
BEGIN
    DECLARE v_current_status VARCHAR(30);
    DECLARE v_trip_status VARCHAR(30);
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @text = MESSAGE_TEXT;
        SET p_response_code = -1;
        SET p_response_message = CONCAT('Lỗi: ', @text);
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Kiểm tra vé tồn tại và trạng thái
    SELECT v.trang_thai_tt, c.trang_thai 
    INTO v_current_status, v_trip_status
    FROM ve v
    JOIN chuyen_xe c ON v.ma_chuyen = c.ma_chuyen
    WHERE v.stt = p_stt;
    
    IF v_current_status IS NULL THEN
        SET p_response_code = -3;
        SET p_response_message = 'Không tìm thấy vé với mã này';
        ROLLBACK;
    ELSEIF v_current_status = 'Cancelled' THEN
        SET p_response_code = -2;
        SET p_response_message = 'Vé này đã được hủy trước đó';
        ROLLBACK;
    ELSEIF v_trip_status = 'Completed' THEN
        SET p_response_code = -2;
        SET p_response_message = 'Không thể hủy vé của chuyến đã hoàn thành';
        ROLLBACK;
    ELSEIF v_trip_status = 'InProgress' THEN
        SET p_response_code = -2;
        SET p_response_message = 'Không thể hủy vé khi chuyến đang chạy';
        ROLLBACK;
    ELSE
        UPDATE ve SET trang_thai_tt = 'Cancelled' WHERE stt = p_stt;
        
        SET p_response_code = 0;
        SET p_response_message = 'Hủy vé thành công';
        COMMIT;
    END IF;
END //
DELIMITER ;

-- =============================================================================
-- SEARCH MODULE: Universal Search Procedures
-- =============================================================================

-- Generic Bus Search
DROP PROCEDURE IF EXISTS sp_bus_search;

DELIMITER //
CREATE PROCEDURE sp_bus_search(
    IN p_keyword VARCHAR(100),
    IN p_status VARCHAR(30),
    IN p_hang_san_xuat VARCHAR(50)
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
        x.tong_km_van_hanh,
        x.ngay_bao_tri_cuoi,
        x.so_cho_ngoi
    FROM xe x
    WHERE (p_keyword IS NULL OR p_keyword = '' 
           OR x.ten_xe LIKE CONCAT('%', p_keyword, '%')
           OR x.bien_so LIKE CONCAT('%', p_keyword, '%')
           OR x.ma_xe LIKE CONCAT('%', p_keyword, '%'))
      AND (p_status IS NULL OR p_status = '' OR x.trang_thai = p_status)
      AND (p_hang_san_xuat IS NULL OR p_hang_san_xuat = '' OR x.hang_san_xuat = p_hang_san_xuat)
    ORDER BY x.ten_xe;
END //
DELIMITER ;

-- Generic Driver Search
DROP PROCEDURE IF EXISTS sp_driver_search;

DELIMITER //
CREATE PROCEDURE sp_driver_search(
    IN p_keyword VARCHAR(100),
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100)
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.ngay_sinh,
        tx.gioi_tinh,
        tx.que_quan,
        tx.so_cccd,
        tx.ngay_ky_hop_dong,
        tx.tuoi,
        tx.he_so_luong,
        tx.tong_so_chuyen
    FROM tai_xe tx
    WHERE (p_keyword IS NULL OR p_keyword = ''
           OR tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%')
           OR tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%')
           OR tx.so_cccd LIKE CONCAT('%', p_keyword, '%'))
      AND (p_gioi_tinh IS NULL OR p_gioi_tinh = '' OR tx.gioi_tinh = p_gioi_tinh)
      AND (p_que_quan IS NULL OR p_que_quan = '' OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'))
    ORDER BY tx.ten_tai_xe;
END //
DELIMITER ;

-- Generic Route Search
DROP PROCEDURE IF EXISTS sp_route_search;

DELIMITER //
CREATE PROCEDURE sp_route_search(
    IN p_keyword VARCHAR(100),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100)
)
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap,
        dpt.ten_muc AS ten_do_phuc_tap
    FROM tuyen_duong td
    LEFT JOIN do_phuc_tap dpt ON td.ma_do_phuc_tap = dpt.ma_do_phuc_tap
    WHERE (p_keyword IS NULL OR p_keyword = ''
           OR td.ten_tuyen LIKE CONCAT('%', p_keyword, '%')
           OR td.ma_tuyen LIKE CONCAT('%', p_keyword, '%'))
      AND (p_diem_di IS NULL OR p_diem_di = '' OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
      AND (p_diem_den IS NULL OR p_diem_den = '' OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'))
    ORDER BY td.ten_tuyen;
END //
DELIMITER ;

-- Trip Search
DROP PROCEDURE IF EXISTS sp_trip_search;

DELIMITER //
CREATE PROCEDURE sp_trip_search(
    IN p_keyword VARCHAR(100),
    IN p_trang_thai VARCHAR(30),
    IN p_date_from DATETIME,
    IN p_date_to DATETIME
)
BEGIN
    SELECT 
        cx.ma_chuyen,
        cx.ten_chuyen,
        cx.thoi_gian_khoi_hanh,
        cx.thoi_gian_den,
        cx.ma_xe,
        x.ten_xe,
        x.bien_so,
        cx.ma_tuyen,
        td.ten_tuyen,
        cx.trang_thai,
        (SELECT COUNT(*) FROM ve v WHERE v.ma_chuyen = cx.ma_chuyen AND v.trang_thai_tt != 'Cancelled') AS so_ve_da_ban,
        COALESCE(x.so_cho_ngoi, 45) AS tong_cho
    FROM chuyen_xe cx
    LEFT JOIN xe x ON cx.ma_xe = x.ma_xe
    LEFT JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    WHERE (p_keyword IS NULL OR p_keyword = ''
           OR cx.ten_chuyen LIKE CONCAT('%', p_keyword, '%')
           OR cx.ma_chuyen LIKE CONCAT('%', p_keyword, '%'))
      AND (p_trang_thai IS NULL OR p_trang_thai = '' OR cx.trang_thai = p_trang_thai)
      AND (p_date_from IS NULL OR cx.thoi_gian_khoi_hanh >= p_date_from)
      AND (p_date_to IS NULL OR cx.thoi_gian_khoi_hanh <= p_date_to)
    ORDER BY cx.thoi_gian_khoi_hanh DESC;
END //
DELIMITER ;

-- =============================================================================
-- Verify procedures created
-- =============================================================================
SHOW PROCEDURE STATUS WHERE Db = 'quan_ly_hang_van_tai_hanh_khach_duong_dai';
