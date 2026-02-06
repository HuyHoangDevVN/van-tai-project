-- =============================================================================
-- ALL STORED PROCEDURES - COMBINED FILE
-- =============================================================================
-- Ch?y file n�y trong DBeaver ho?c MySQL Workbench
-- Total: 38 stored procedures
-- =============================================================================

-- =============================================================================
-- REPORT MODULE (9 procedures)
-- =============================================================================

-- proc_tinh_chi_phi_co_ban
DROP PROCEDURE IF EXISTS proc_tinh_chi_phi_co_ban;
CREATE PROCEDURE proc_tinh_chi_phi_co_ban()
BEGIN
    SELECT 
        cx.ma_chuyen,
        td.ten_tuyen,
        td.khoang_cach,
        x.muc_tieu_hao,
        (td.khoang_cach * x.muc_tieu_hao * 25000) AS chi_phi_nhien_lieu,
        x.phu_thu_phi_van_hanh AS chi_phi_van_hanh,
        (td.khoang_cach * x.muc_tieu_hao * 25000 + x.phu_thu_phi_van_hanh) AS tong_chi_phi
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    ORDER BY cx.ma_chuyen;
END;

-- proc_tinh_doanh_thu_xe_bus_ngoi
DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_xe_bus_ngoi;
CREATE PROCEDURE proc_tinh_doanh_thu_xe_bus_ngoi()
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM xe x
    INNER JOIN chuyen_xe cx ON x.ma_xe = cx.ma_xe
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN gia g ON cx.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE x.ma_loai_xe = 'BUS_NGOI'
    GROUP BY x.ma_xe, x.bien_so, x.ten_xe
    ORDER BY tong_doanh_thu DESC;
END;

-- proc_tinh_doanh_thu_xe_bus_ngoi_thang
DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_xe_bus_ngoi_thang;
CREATE PROCEDURE proc_tinh_doanh_thu_xe_bus_ngoi_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        MONTH(cx.ngay_khoi_hanh) AS thang,
        YEAR(cx.ngay_khoi_hanh) AS nam,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM xe x
    INNER JOIN chuyen_xe cx ON x.ma_xe = cx.ma_xe
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN gia g ON cx.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE x.ma_loai_xe = 'BUS_NGOI'
        AND cx.ngay_khoi_hanh >= p_tu
        AND cx.ngay_khoi_hanh <= p_den
    GROUP BY x.ma_xe, x.bien_so, x.ten_xe, YEAR(cx.ngay_khoi_hanh), MONTH(cx.ngay_khoi_hanh)
    ORDER BY nam, thang, tong_doanh_thu DESC;
END;

-- proc_tinh_gia_ve_xe_bus_ngoi
DROP PROCEDURE IF EXISTS proc_tinh_gia_ve_xe_bus_ngoi;
CREATE PROCEDURE proc_tinh_gia_ve_xe_bus_ngoi()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        g.gia AS gia_ve
    FROM tuyen_duong td
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen
    WHERE g.ma_loai_xe = 'BUS_NGOI'
    ORDER BY td.ten_tuyen;
END;

-- proc_tinh_doanh_thu_tuyen_duong
DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_tuyen_duong;
CREATE PROCEDURE proc_tinh_doanh_thu_tuyen_duong()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COUNT(v.ma_ve) AS so_ve,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM tuyen_duong td
    LEFT JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    LEFT JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    LEFT JOIN xe x ON cx.ma_xe = x.ma_xe
    LEFT JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    GROUP BY td.ma_tuyen, td.ten_tuyen, td.diem_di, td.diem_den, td.khoang_cach
    ORDER BY tong_doanh_thu DESC;
END;

-- proc_tinh_doanh_thu_tuyen_duong_thang
DROP PROCEDURE IF EXISTS proc_tinh_doanh_thu_tuyen_duong_thang;
CREATE PROCEDURE proc_tinh_doanh_thu_tuyen_duong_thang(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        MONTH(cx.ngay_khoi_hanh) AS thang,
        YEAR(cx.ngay_khoi_hanh) AS nam,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        COALESCE(SUM(g.gia), 0) AS tong_doanh_thu
    FROM tuyen_duong td
    INNER JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    INNER JOIN ve v ON cx.ma_chuyen = v.ma_chuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE cx.ngay_khoi_hanh >= p_tu
        AND cx.ngay_khoi_hanh <= p_den
    GROUP BY td.ma_tuyen, td.ten_tuyen, YEAR(cx.ngay_khoi_hanh), MONTH(cx.ngay_khoi_hanh)
    ORDER BY nam, thang, tong_doanh_thu DESC;
END;

-- proc_tinh_luong_thang_tai_xe
DROP PROCEDURE IF EXISTS proc_tinh_luong_thang_tai_xe;
CREATE PROCEDURE proc_tinh_luong_thang_tai_xe(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    DECLARE v_luong_co_ban DECIMAL(15,2) DEFAULT 10000000;
    DECLARE v_phu_cap_km DECIMAL(10,2) DEFAULT 5000;

    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.he_so_luong,
        COALESCE(SUM(td.khoang_cach), 0) AS tong_km,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen,
        (v_luong_co_ban * tx.he_so_luong) AS luong_co_ban,
        (COALESCE(SUM(td.khoang_cach), 0) * v_phu_cap_km) AS phu_cap,
        ((v_luong_co_ban * tx.he_so_luong) + (COALESCE(SUM(td.khoang_cach), 0) * v_phu_cap_km)) AS luong_thang
    FROM tai_xe tx
    LEFT JOIN phan_cong_tai_xe pctx ON tx.ma_tai_xe = pctx.ma_tai_xe
    LEFT JOIN chuyen_xe cx ON pctx.ma_xe = cx.ma_xe
        AND cx.ngay_khoi_hanh >= p_tu
        AND cx.ngay_khoi_hanh <= p_den
    LEFT JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    GROUP BY tx.ma_tai_xe, tx.ten_tai_xe, tx.he_so_luong
    ORDER BY luong_thang DESC;
END;

-- proc_lay_tuyen_duong_xe_giuong_nam
DROP PROCEDURE IF EXISTS proc_lay_tuyen_duong_xe_giuong_nam;
CREATE PROCEDURE proc_lay_tuyen_duong_xe_giuong_nam()
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        g.gia AS gia_ve,
        COUNT(DISTINCT cx.ma_chuyen) AS so_chuyen
    FROM tuyen_duong td
    INNER JOIN gia g ON td.ma_tuyen = g.ma_tuyen
    LEFT JOIN chuyen_xe cx ON td.ma_tuyen = cx.ma_tuyen
    LEFT JOIN xe x ON cx.ma_xe = x.ma_xe AND x.ma_loai_xe = 'GIUONG_NAM'
    WHERE g.ma_loai_xe = 'GIUONG_NAM'
    GROUP BY td.ma_tuyen, td.ten_tuyen, td.diem_di, td.diem_den, td.khoang_cach, g.gia
    ORDER BY td.ten_tuyen;
END;

-- proc_lay_khach_hang_quay_lai
DROP PROCEDURE IF EXISTS proc_lay_khach_hang_quay_lai;
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
    HAVING COUNT(v.ma_ve) > 1
    ORDER BY so_lan_dat_ve DESC, tong_chi_tieu DESC;
END;

-- =============================================================================
-- MAINTENANCE MODULE (4 procedures)
-- =============================================================================

-- proc_kiem_tra_trang_thai_bao_tri
DROP PROCEDURE IF EXISTS proc_kiem_tra_trang_thai_bao_tri;
CREATE PROCEDURE proc_kiem_tra_trang_thai_bao_tri(
    IN p_ma_xe VARCHAR(20)
)
BEGIN
    SELECT 
        x.ma_xe,
        x.bien_so,
        x.ten_xe,
        x.trang_thai,
        bt.ngay_bao_tri AS ngay_bao_tri_gan_nhat,
        bt.noi_dung AS noi_dung_bao_tri,
        bt.chi_phi AS chi_phi_bao_tri,
        DATE_ADD(bt.ngay_bao_tri, INTERVAL 6 MONTH) AS ngay_bao_tri_tiep_theo,
        CASE 
            WHEN bt.ngay_bao_tri IS NULL THEN 'Chua co lich su bao tri'
            WHEN DATE_ADD(bt.ngay_bao_tri, INTERVAL 6 MONTH) < CURDATE() THEN 'Can bao tri'
            WHEN DATE_ADD(bt.ngay_bao_tri, INTERVAL 5 MONTH) < CURDATE() THEN 'Sap den han bao tri'
            ELSE 'Binh thuong'
        END AS trang_thai_bao_tri
    FROM xe x
    LEFT JOIN (
        SELECT ma_xe, ngay_bao_tri, noi_dung, chi_phi
        FROM bao_tri
        WHERE (ma_xe, ngay_bao_tri) IN (
            SELECT ma_xe, MAX(ngay_bao_tri)
            FROM bao_tri
            GROUP BY ma_xe
        )
    ) bt ON x.ma_xe = bt.ma_xe
    WHERE (p_ma_xe IS NULL OR x.ma_xe = p_ma_xe)
    ORDER BY x.ma_xe;
END;

-- proc_tao_bao_tri
DROP PROCEDURE IF EXISTS proc_tao_bao_tri;
CREATE PROCEDURE proc_tao_bao_tri(
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_bao_tri DATETIME,
    IN p_noi_dung TEXT,
    IN p_chi_phi DECIMAL(15,2)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe khong ton tai';
    END IF;

    INSERT INTO bao_tri (ma_xe, ngay_bao_tri, noi_dung, chi_phi)
    VALUES (p_ma_xe, p_ngay_bao_tri, p_noi_dung, p_chi_phi);

    UPDATE xe 
    SET trang_thai = 'Dang bao tri'
    WHERE ma_xe = p_ma_xe 
        AND p_ngay_bao_tri = CURDATE();

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_lay_lich_su_bao_tri
DROP PROCEDURE IF EXISTS proc_lay_lich_su_bao_tri;
CREATE PROCEDURE proc_lay_lich_su_bao_tri(
    IN p_ma_xe VARCHAR(20),
    IN p_tu_ngay DATETIME,
    IN p_den_ngay DATETIME
)
BEGIN
    SELECT 
        bt.ma_bao_tri,
        bt.ma_xe,
        x.bien_so,
        x.ten_xe,
        bt.ngay_bao_tri,
        bt.noi_dung,
        bt.chi_phi
    FROM bao_tri bt
    INNER JOIN xe x ON bt.ma_xe = x.ma_xe
    WHERE (p_ma_xe IS NULL OR bt.ma_xe = p_ma_xe)
        AND (p_tu_ngay IS NULL OR bt.ngay_bao_tri >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR bt.ngay_bao_tri <= p_den_ngay)
    ORDER BY bt.ngay_bao_tri DESC;
END;

-- proc_cap_nhat_bao_tri
DROP PROCEDURE IF EXISTS proc_cap_nhat_bao_tri;
CREATE PROCEDURE proc_cap_nhat_bao_tri(
    IN p_ma_bao_tri INT,
    IN p_ngay_bao_tri DATETIME,
    IN p_noi_dung TEXT,
    IN p_chi_phi DECIMAL(15,2)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM bao_tri WHERE ma_bao_tri = p_ma_bao_tri) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ban ghi bao tri khong ton tai';
    END IF;

    UPDATE bao_tri
    SET 
        ngay_bao_tri = COALESCE(p_ngay_bao_tri, ngay_bao_tri),
        noi_dung = COALESCE(p_noi_dung, noi_dung),
        chi_phi = COALESCE(p_chi_phi, chi_phi)
    WHERE ma_bao_tri = p_ma_bao_tri;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- =============================================================================
-- BUS MODULE (6 procedures)
-- =============================================================================

-- proc_tim_kiem_xe
DROP PROCEDURE IF EXISTS proc_tim_kiem_xe;
CREATE PROCEDURE proc_tim_kiem_xe(
    IN p_keyword VARCHAR(200),
    IN p_status VARCHAR(30),
    IN p_hang_san_xuat VARCHAR(50),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM xe x
    WHERE 
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_status IS NULL OR x.trang_thai = p_status)
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat);

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
        x.ma_loai_xe
    FROM xe x
    WHERE 
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_status IS NULL OR x.trang_thai = p_status)
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat)
    ORDER BY
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 1 THEN x.bien_so END DESC,
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 0 THEN x.bien_so END ASC,
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 1 THEN x.ten_xe END DESC,
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 0 THEN x.ten_xe END ASC,
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 1 THEN x.nam_san_xuat END DESC,
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 0 THEN x.nam_san_xuat END ASC,
        x.ma_xe DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_xe_theo_id
DROP PROCEDURE IF EXISTS proc_lay_xe_theo_id;
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
END;

-- proc_tao_xe
DROP PROCEDURE IF EXISTS proc_tao_xe;
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
    IF EXISTS (SELECT 1 FROM xe WHERE bien_so = p_bien_so) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bien so xe da ton tai';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM loai_xe WHERE ma_loai_xe = p_ma_loai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Loai xe khong ton tai';
    END IF;

    INSERT INTO xe (
        ma_xe, ten_xe, bien_so, hang_san_xuat, nam_san_xuat,
        ngay_dang_kiem, trang_thai, muc_tieu_hao, phu_thu_phi_van_hanh, ma_loai_xe
    )
    VALUES (
        p_ma_xe, p_ten_xe, p_bien_so, p_hang_san_xuat, p_nam_san_xuat,
        p_ngay_dang_kiem, COALESCE(p_trang_thai, 'Hoat dong'), p_muc_tieu_hao, 
        p_phu_thu_phi_van_hanh, p_ma_loai_xe
    );

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_cap_nhat_xe
DROP PROCEDURE IF EXISTS proc_cap_nhat_xe;
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
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe khong ton tai';
    END IF;

    IF EXISTS (SELECT 1 FROM xe WHERE bien_so = p_bien_so AND ma_xe != p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bien so xe da duoc su dung boi xe khac';
    END IF;

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
END;

-- proc_xoa_xe
DROP PROCEDURE IF EXISTS proc_xoa_xe;
CREATE PROCEDURE proc_xoa_xe(
    IN p_ma_xe VARCHAR(20)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe khong ton tai';
    END IF;

    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_xe = p_ma_xe 
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khong the xoa xe dang co chuyen xe hoat dong';
    END IF;

    UPDATE xe
    SET trang_thai = 'Da xoa'
    WHERE ma_xe = p_ma_xe;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_lay_xe_san_sang
DROP PROCEDURE IF EXISTS proc_lay_xe_san_sang;
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
    WHERE x.trang_thai = 'Hoat dong'
        AND x.ma_xe NOT IN (
            SELECT DISTINCT ma_xe 
            FROM chuyen_xe 
            WHERE DATE(ngay_khoi_hanh) = COALESCE(p_ngay, CURDATE())
                AND trang_thai IN ('Scheduled', 'In Progress')
        )
        AND x.ma_xe NOT IN (
            SELECT DISTINCT ma_xe 
            FROM bao_tri 
            WHERE DATE(ngay_bao_tri) = COALESCE(p_ngay, CURDATE())
        )
    ORDER BY x.ten_xe;
END;

-- =============================================================================
-- DRIVER MODULE (6 procedures)
-- =============================================================================

-- proc_tim_kiem_tai_xe
DROP PROCEDURE IF EXISTS proc_tim_kiem_tai_xe;
CREATE PROCEDURE proc_tim_kiem_tai_xe(
    IN p_keyword VARCHAR(200),
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM tai_xe tx
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'));

    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.ngay_sinh,
        tx.gioi_tinh,
        tx.que_quan,
        tx.ton_giao,
        tx.so_cccd,
        tx.ngay_ky_hop_dong,
        tx.tuoi,
        tx.he_so_luong
    FROM tai_xe tx
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 1 THEN tx.ten_tai_xe END DESC,
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 0 THEN tx.ten_tai_xe END ASC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 1 THEN tx.ngay_ky_hop_dong END DESC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 0 THEN tx.ngay_ky_hop_dong END ASC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 1 THEN tx.he_so_luong END DESC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 0 THEN tx.he_so_luong END ASC,
        tx.ma_tai_xe DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_tai_xe_theo_id
DROP PROCEDURE IF EXISTS proc_lay_tai_xe_theo_id;
CREATE PROCEDURE proc_lay_tai_xe_theo_id(
    IN p_ma_tai_xe VARCHAR(20)
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.ngay_sinh,
        tx.gioi_tinh,
        tx.que_quan,
        tx.ton_giao,
        tx.so_cccd,
        tx.ngay_ky_hop_dong,
        tx.tuoi,
        tx.he_so_luong
    FROM tai_xe tx
    WHERE tx.ma_tai_xe = p_ma_tai_xe;
END;

-- proc_tao_tai_xe
DROP PROCEDURE IF EXISTS proc_tao_tai_xe;
CREATE PROCEDURE proc_tao_tai_xe(
    IN p_ma_tai_xe VARCHAR(20),
    IN p_ten_tai_xe VARCHAR(100),
    IN p_ngay_sinh DATE,
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_ton_giao VARCHAR(50),
    IN p_so_cccd VARCHAR(20),
    IN p_ngay_ky_hop_dong DATE,
    IN p_he_so_luong DECIMAL(5,2)
)
BEGIN
    DECLARE v_tuoi INT;
    
    IF EXISTS (SELECT 1 FROM tai_xe WHERE so_cccd = p_so_cccd) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'So CCCD da ton tai trong he thong';
    END IF;

    SET v_tuoi = TIMESTAMPDIFF(YEAR, p_ngay_sinh, CURDATE());

    IF v_tuoi < 18 OR v_tuoi > 60 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuoi tai xe phai tu 18 den 60';
    END IF;

    INSERT INTO tai_xe (
        ma_tai_xe, ten_tai_xe, ngay_sinh, gioi_tinh, que_quan,
        ton_giao, so_cccd, ngay_ky_hop_dong, tuoi, he_so_luong
    )
    VALUES (
        p_ma_tai_xe, p_ten_tai_xe, p_ngay_sinh, p_gioi_tinh, p_que_quan,
        p_ton_giao, p_so_cccd, p_ngay_ky_hop_dong, v_tuoi, COALESCE(p_he_so_luong, 1.0)
    );

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_cap_nhat_tai_xe
DROP PROCEDURE IF EXISTS proc_cap_nhat_tai_xe;
CREATE PROCEDURE proc_cap_nhat_tai_xe(
    IN p_ma_tai_xe VARCHAR(20),
    IN p_ten_tai_xe VARCHAR(100),
    IN p_ngay_sinh DATE,
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_ton_giao VARCHAR(50),
    IN p_so_cccd VARCHAR(20),
    IN p_ngay_ky_hop_dong DATE,
    IN p_he_so_luong DECIMAL(5,2)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tai xe khong ton tai';
    END IF;

    IF p_so_cccd IS NOT NULL AND EXISTS (
        SELECT 1 FROM tai_xe WHERE so_cccd = p_so_cccd AND ma_tai_xe != p_ma_tai_xe
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'So CCCD da duoc su dung boi tai xe khac';
    END IF;

    UPDATE tai_xe
    SET 
        ten_tai_xe = COALESCE(p_ten_tai_xe, ten_tai_xe),
        ngay_sinh = COALESCE(p_ngay_sinh, ngay_sinh),
        gioi_tinh = COALESCE(p_gioi_tinh, gioi_tinh),
        que_quan = COALESCE(p_que_quan, que_quan),
        ton_giao = COALESCE(p_ton_giao, ton_giao),
        so_cccd = COALESCE(p_so_cccd, so_cccd),
        ngay_ky_hop_dong = COALESCE(p_ngay_ky_hop_dong, ngay_ky_hop_dong),
        tuoi = CASE WHEN p_ngay_sinh IS NOT NULL THEN TIMESTAMPDIFF(YEAR, p_ngay_sinh, CURDATE()) ELSE tuoi END,
        he_so_luong = COALESCE(p_he_so_luong, he_so_luong)
    WHERE ma_tai_xe = p_ma_tai_xe;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_xoa_tai_xe
DROP PROCEDURE IF EXISTS proc_xoa_tai_xe;
CREATE PROCEDURE proc_xoa_tai_xe(
    IN p_ma_tai_xe VARCHAR(20)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tai xe khong ton tai';
    END IF;

    IF EXISTS (
        SELECT 1 FROM phan_cong_tai_xe pctx
        INNER JOIN chuyen_xe cx ON pctx.ma_xe = cx.ma_xe
        WHERE pctx.ma_tai_xe = p_ma_tai_xe 
        AND cx.trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khong the xoa tai xe dang duoc phan cong chuyen xe';
    END IF;

    DELETE FROM phan_cong_tai_xe WHERE ma_tai_xe = p_ma_tai_xe;
    DELETE FROM tai_xe WHERE ma_tai_xe = p_ma_tai_xe;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_lay_tai_xe_san_sang
DROP PROCEDURE IF EXISTS proc_lay_tai_xe_san_sang;
CREATE PROCEDURE proc_lay_tai_xe_san_sang(
    IN p_ngay DATE
)
BEGIN
    SELECT 
        tx.ma_tai_xe,
        tx.ten_tai_xe,
        tx.gioi_tinh,
        tx.so_cccd,
        tx.he_so_luong
    FROM tai_xe tx
    WHERE tx.ma_tai_xe NOT IN (
        SELECT DISTINCT pctx.ma_tai_xe 
        FROM phan_cong_tai_xe pctx
        INNER JOIN chuyen_xe cx ON pctx.ma_xe = cx.ma_xe
        WHERE DATE(cx.ngay_khoi_hanh) = COALESCE(p_ngay, CURDATE())
            AND cx.trang_thai IN ('Scheduled', 'In Progress')
    )
    ORDER BY tx.ten_tai_xe;
END;

-- =============================================================================
-- ROUTE MODULE (5 procedures)
-- =============================================================================

-- proc_tim_kiem_tuyen_duong
DROP PROCEDURE IF EXISTS proc_tim_kiem_tuyen_duong;
CREATE PROCEDURE proc_tim_kiem_tuyen_duong(
    IN p_keyword VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'));

    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 1 THEN td.ten_tuyen END DESC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 0 THEN td.ten_tuyen END ASC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 1 THEN td.khoang_cach END DESC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 0 THEN td.khoang_cach END ASC,
        td.ma_tuyen DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_tuyen_duong_theo_id
DROP PROCEDURE IF EXISTS proc_lay_tuyen_duong_theo_id;
CREATE PROCEDURE proc_lay_tuyen_duong_theo_id(
    IN p_ma_tuyen VARCHAR(20)
)
BEGIN
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap
    FROM tuyen_duong td
    WHERE td.ma_tuyen = p_ma_tuyen;
END;

-- proc_tao_tuyen_duong
DROP PROCEDURE IF EXISTS proc_tao_tuyen_duong;
CREATE PROCEDURE proc_tao_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20),
    IN p_ten_tuyen VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_khoang_cach DECIMAL(10,2),
    IN p_ma_do_phuc_tap VARCHAR(20)
)
BEGIN
    IF EXISTS (SELECT 1 FROM tuyen_duong WHERE diem_di = p_diem_di AND diem_den = p_diem_den) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuyen duong voi diem di va diem den nay da ton tai';
    END IF;

    INSERT INTO tuyen_duong (
        ma_tuyen, ten_tuyen, diem_di, diem_den, khoang_cach, ma_do_phuc_tap
    )
    VALUES (
        p_ma_tuyen, p_ten_tuyen, p_diem_di, p_diem_den, p_khoang_cach, p_ma_do_phuc_tap
    );

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_cap_nhat_tuyen_duong
DROP PROCEDURE IF EXISTS proc_cap_nhat_tuyen_duong;
CREATE PROCEDURE proc_cap_nhat_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20),
    IN p_ten_tuyen VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_khoang_cach DECIMAL(10,2),
    IN p_ma_do_phuc_tap VARCHAR(20)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuyen duong khong ton tai';
    END IF;

    UPDATE tuyen_duong
    SET 
        ten_tuyen = COALESCE(p_ten_tuyen, ten_tuyen),
        diem_di = COALESCE(p_diem_di, diem_di),
        diem_den = COALESCE(p_diem_den, diem_den),
        khoang_cach = COALESCE(p_khoang_cach, khoang_cach),
        ma_do_phuc_tap = COALESCE(p_ma_do_phuc_tap, ma_do_phuc_tap)
    WHERE ma_tuyen = p_ma_tuyen;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_xoa_tuyen_duong
DROP PROCEDURE IF EXISTS proc_xoa_tuyen_duong;
CREATE PROCEDURE proc_xoa_tuyen_duong(
    IN p_ma_tuyen VARCHAR(20)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuyen duong khong ton tai';
    END IF;

    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_tuyen = p_ma_tuyen 
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khong the xoa tuyen duong dang co chuyen xe hoat dong';
    END IF;

    DELETE FROM gia WHERE ma_tuyen = p_ma_tuyen;
    DELETE FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- =============================================================================
-- TRIP MODULE (7 procedures)
-- =============================================================================

-- proc_tim_kiem_chuyen_xe
DROP PROCEDURE IF EXISTS proc_tim_kiem_chuyen_xe;
CREATE PROCEDURE proc_tim_kiem_chuyen_xe(
    IN p_keyword VARCHAR(200),
    IN p_trang_thai VARCHAR(30),
    IN p_ma_tuyen VARCHAR(20),
    IN p_tu_ngay DATETIME,
    IN p_den_ngay DATETIME,
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE 
        (p_keyword IS NULL OR (
            cx.ma_chuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR cx.trang_thai = p_trang_thai)
        AND (p_ma_tuyen IS NULL OR cx.ma_tuyen = p_ma_tuyen)
        AND (p_tu_ngay IS NULL OR cx.ngay_khoi_hanh >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR cx.ngay_khoi_hanh <= p_den_ngay);

    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        cx.ngay_khoi_hanh,
        cx.trang_thai
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE 
        (p_keyword IS NULL OR (
            cx.ma_chuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR cx.trang_thai = p_trang_thai)
        AND (p_ma_tuyen IS NULL OR cx.ma_tuyen = p_ma_tuyen)
        AND (p_tu_ngay IS NULL OR cx.ngay_khoi_hanh >= p_tu_ngay)
        AND (p_den_ngay IS NULL OR cx.ngay_khoi_hanh <= p_den_ngay)
    ORDER BY
        CASE WHEN p_sort_by = 'ngay_khoi_hanh' AND p_sort_desc = 1 THEN cx.ngay_khoi_hanh END DESC,
        CASE WHEN p_sort_by = 'ngay_khoi_hanh' AND p_sort_desc = 0 THEN cx.ngay_khoi_hanh END ASC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 1 THEN td.ten_tuyen END DESC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 0 THEN td.ten_tuyen END ASC,
        cx.ma_chuyen DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_chuyen_xe_theo_id
DROP PROCEDURE IF EXISTS proc_lay_chuyen_xe_theo_id;
CREATE PROCEDURE proc_lay_chuyen_xe_theo_id(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        x.ma_loai_xe,
        lx.ten_loai_xe,
        lx.so_cho,
        cx.ngay_khoi_hanh,
        cx.trang_thai,
        (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen) AS so_ve_da_ban,
        (lx.so_cho - (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen)) AS so_ghe_trong
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    LEFT JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE cx.ma_chuyen = p_ma_chuyen;
END;

-- proc_tao_chuyen_xe
DROP PROCEDURE IF EXISTS proc_tao_chuyen_xe;
CREATE PROCEDURE proc_tao_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_tuyen VARCHAR(20),
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_khoi_hanh DATETIME,
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tuyen_duong WHERE ma_tuyen = p_ma_tuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tuyen duong khong ton tai';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM xe WHERE ma_xe = p_ma_xe AND trang_thai = 'Hoat dong') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe khong ton tai hoac khong dang hoat dong';
    END IF;

    IF EXISTS (
        SELECT 1 FROM chuyen_xe 
        WHERE ma_xe = p_ma_xe 
        AND DATE(ngay_khoi_hanh) = DATE(p_ngay_khoi_hanh)
        AND trang_thai IN ('Scheduled', 'In Progress')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xe da co chuyen khac trong ngay nay';
    END IF;

    INSERT INTO chuyen_xe (
        ma_chuyen, ma_tuyen, ma_xe, ngay_khoi_hanh, trang_thai
    )
    VALUES (
        p_ma_chuyen, p_ma_tuyen, p_ma_xe, p_ngay_khoi_hanh, COALESCE(p_trang_thai, 'Scheduled')
    );

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_cap_nhat_chuyen_xe
DROP PROCEDURE IF EXISTS proc_cap_nhat_chuyen_xe;
CREATE PROCEDURE proc_cap_nhat_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_tuyen VARCHAR(20),
    IN p_ma_xe VARCHAR(20),
    IN p_ngay_khoi_hanh DATETIME,
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuyen xe khong ton tai';
    END IF;

    UPDATE chuyen_xe
    SET 
        ma_tuyen = COALESCE(p_ma_tuyen, ma_tuyen),
        ma_xe = COALESCE(p_ma_xe, ma_xe),
        ngay_khoi_hanh = COALESCE(p_ngay_khoi_hanh, ngay_khoi_hanh),
        trang_thai = COALESCE(p_trang_thai, trang_thai)
    WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_xoa_chuyen_xe
DROP PROCEDURE IF EXISTS proc_xoa_chuyen_xe;
CREATE PROCEDURE proc_xoa_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuyen xe khong ton tai';
    END IF;

    IF EXISTS (SELECT 1 FROM ve WHERE ma_chuyen = p_ma_chuyen) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khong the xoa chuyen xe da co ve duoc dat';
    END IF;

    DELETE FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_hoan_thanh_chuyen_xe
DROP PROCEDURE IF EXISTS proc_hoan_thanh_chuyen_xe;
CREATE PROCEDURE proc_hoan_thanh_chuyen_xe(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    DECLARE v_trang_thai_hien_tai VARCHAR(30);

    SELECT trang_thai INTO v_trang_thai_hien_tai
    FROM chuyen_xe 
    WHERE ma_chuyen = p_ma_chuyen;

    IF v_trang_thai_hien_tai IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuyen xe khong ton tai';
    END IF;

    IF v_trang_thai_hien_tai NOT IN ('Scheduled', 'In Progress') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chi co the hoan thanh chuyen xe dang len lich hoac dang chay';
    END IF;

    UPDATE chuyen_xe
    SET trang_thai = 'Completed'
    WHERE ma_chuyen = p_ma_chuyen;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_lay_chuyen_xe_theo_trang_thai
DROP PROCEDURE IF EXISTS proc_lay_chuyen_xe_theo_trang_thai;
CREATE PROCEDURE proc_lay_chuyen_xe_theo_trang_thai(
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    SELECT 
        cx.ma_chuyen,
        cx.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        cx.ma_xe,
        x.bien_so,
        x.ten_xe,
        cx.ngay_khoi_hanh,
        cx.trang_thai,
        (SELECT COUNT(*) FROM ve WHERE ma_chuyen = cx.ma_chuyen) AS so_ve_da_ban
    FROM chuyen_xe cx
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    WHERE cx.trang_thai = p_trang_thai
    ORDER BY cx.ngay_khoi_hanh DESC;
END;

-- =============================================================================
-- TICKET MODULE (6 procedures)
-- =============================================================================

-- proc_tim_kiem_ve
DROP PROCEDURE IF EXISTS proc_tim_kiem_ve;
CREATE PROCEDURE proc_tim_kiem_ve(
    IN p_keyword VARCHAR(200),
    IN p_trang_thai VARCHAR(30),
    IN p_ma_chuyen VARCHAR(20),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE 
        (p_keyword IS NULL OR (
            v.ma_ve LIKE CONCAT('%', p_keyword, '%') OR
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR v.trang_thai = p_trang_thai)
        AND (p_ma_chuyen IS NULL OR v.ma_chuyen = p_ma_chuyen);

    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai,
        cx.ngay_khoi_hanh,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE 
        (p_keyword IS NULL OR (
            v.ma_ve LIKE CONCAT('%', p_keyword, '%') OR
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_trang_thai IS NULL OR v.trang_thai = p_trang_thai)
        AND (p_ma_chuyen IS NULL OR v.ma_chuyen = p_ma_chuyen)
    ORDER BY
        CASE WHEN p_sort_by = 'ngay_dat' AND p_sort_desc = 1 THEN v.ngay_dat END DESC,
        CASE WHEN p_sort_by = 'ngay_dat' AND p_sort_desc = 0 THEN v.ngay_dat END ASC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 1 THEN kh.ten_khach_hang END DESC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 0 THEN kh.ten_khach_hang END ASC,
        v.ma_ve DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_ve_theo_id
DROP PROCEDURE IF EXISTS proc_lay_ve_theo_id;
CREATE PROCEDURE proc_lay_ve_theo_id(
    IN p_ma_ve VARCHAR(20)
)
BEGIN
    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai,
        cx.ngay_khoi_hanh,
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        x.bien_so,
        x.ten_xe,
        g.gia AS gia_ve
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    INNER JOIN tuyen_duong td ON cx.ma_tuyen = td.ma_tuyen
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    LEFT JOIN gia g ON td.ma_tuyen = g.ma_tuyen AND x.ma_loai_xe = g.ma_loai_xe
    WHERE v.ma_ve = p_ma_ve;
END;

-- proc_dat_ve
DROP PROCEDURE IF EXISTS proc_dat_ve;
CREATE PROCEDURE proc_dat_ve(
    IN p_ma_ve VARCHAR(20),
    IN p_ma_chuyen VARCHAR(20),
    IN p_ma_khach_hang VARCHAR(20),
    IN p_so_ghe VARCHAR(10)
)
BEGIN
    DECLARE v_so_cho INT;
    DECLARE v_so_ve_da_ban INT;

    IF NOT EXISTS (
        SELECT 1 FROM chuyen_xe WHERE ma_chuyen = p_ma_chuyen AND trang_thai = 'Scheduled'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuyen xe khong ton tai hoac khong con nhan dat ve';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM khach_hang WHERE ma_khach_hang = p_ma_khach_hang) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khach hang khong ton tai';
    END IF;

    IF EXISTS (
        SELECT 1 FROM ve WHERE ma_chuyen = p_ma_chuyen AND so_ghe = p_so_ghe AND trang_thai != 'Cancelled'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ghe nay da duoc dat';
    END IF;

    SELECT lx.so_cho INTO v_so_cho
    FROM chuyen_xe cx
    INNER JOIN xe x ON cx.ma_xe = x.ma_xe
    INNER JOIN loai_xe lx ON x.ma_loai_xe = lx.ma_loai_xe
    WHERE cx.ma_chuyen = p_ma_chuyen;

    SELECT COUNT(*) INTO v_so_ve_da_ban
    FROM ve 
    WHERE ma_chuyen = p_ma_chuyen AND trang_thai != 'Cancelled';

    IF v_so_ve_da_ban >= v_so_cho THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chuyen xe da het cho';
    END IF;

    INSERT INTO ve (ma_ve, ma_chuyen, ma_khach_hang, so_ghe, ngay_dat, trang_thai)
    VALUES (p_ma_ve, p_ma_chuyen, p_ma_khach_hang, p_so_ghe, NOW(), 'Booked');

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_huy_ve
DROP PROCEDURE IF EXISTS proc_huy_ve;
CREATE PROCEDURE proc_huy_ve(
    IN p_ma_ve VARCHAR(20)
)
BEGIN
    DECLARE v_trang_thai VARCHAR(30);
    DECLARE v_ngay_khoi_hanh DATETIME;

    SELECT v.trang_thai, cx.ngay_khoi_hanh 
    INTO v_trang_thai, v_ngay_khoi_hanh
    FROM ve v
    INNER JOIN chuyen_xe cx ON v.ma_chuyen = cx.ma_chuyen
    WHERE v.ma_ve = p_ma_ve;

    IF v_trang_thai IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ve khong ton tai';
    END IF;

    IF v_trang_thai = 'Cancelled' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ve da duoc huy truoc do';
    END IF;

    IF v_ngay_khoi_hanh <= NOW() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khong the huy ve cua chuyen xe da khoi hanh';
    END IF;

    UPDATE ve
    SET trang_thai = 'Cancelled'
    WHERE ma_ve = p_ma_ve;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_lay_ve_theo_chuyen
DROP PROCEDURE IF EXISTS proc_lay_ve_theo_chuyen;
CREATE PROCEDURE proc_lay_ve_theo_chuyen(
    IN p_ma_chuyen VARCHAR(20)
)
BEGIN
    SELECT 
        v.ma_ve,
        v.ma_chuyen,
        v.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        v.so_ghe,
        v.ngay_dat,
        v.trang_thai
    FROM ve v
    INNER JOIN khach_hang kh ON v.ma_khach_hang = kh.ma_khach_hang
    WHERE v.ma_chuyen = p_ma_chuyen
    ORDER BY v.so_ghe;
END;

-- proc_cap_nhat_trang_thai_ve
DROP PROCEDURE IF EXISTS proc_cap_nhat_trang_thai_ve;
CREATE PROCEDURE proc_cap_nhat_trang_thai_ve(
    IN p_ma_ve VARCHAR(20),
    IN p_trang_thai VARCHAR(30)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ve WHERE ma_ve = p_ma_ve) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ve khong ton tai';
    END IF;

    IF p_trang_thai NOT IN ('Booked', 'Confirmed', 'Checked-in', 'Completed', 'Cancelled') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Trang thai khong hop le';
    END IF;

    UPDATE ve
    SET trang_thai = p_trang_thai
    WHERE ma_ve = p_ma_ve;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- =============================================================================
-- CUSTOMER MODULE (4 procedures)
-- =============================================================================

-- proc_tim_kiem_khach_hang
DROP PROCEDURE IF EXISTS proc_tim_kiem_khach_hang;
CREATE PROCEDURE proc_tim_kiem_khach_hang(
    IN p_keyword VARCHAR(200),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    OUT p_total_record BIGINT
)
BEGIN
    SELECT COUNT(*) INTO p_total_record
    FROM khach_hang kh
    WHERE 
        (p_keyword IS NULL OR (
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.ma_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%') OR
            kh.email LIKE CONCAT('%', p_keyword, '%')
        ));

    SELECT 
        kh.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        kh.dia_chi
    FROM khach_hang kh
    WHERE 
        (p_keyword IS NULL OR (
            kh.ten_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.ma_khach_hang LIKE CONCAT('%', p_keyword, '%') OR
            kh.so_dien_thoai LIKE CONCAT('%', p_keyword, '%') OR
            kh.email LIKE CONCAT('%', p_keyword, '%')
        ))
    ORDER BY
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 1 THEN kh.ten_khach_hang END DESC,
        CASE WHEN p_sort_by = 'ten_khach_hang' AND p_sort_desc = 0 THEN kh.ten_khach_hang END ASC,
        kh.ma_khach_hang DESC
    LIMIT p_limit OFFSET p_offset;
END;

-- proc_lay_khach_hang_theo_id
DROP PROCEDURE IF EXISTS proc_lay_khach_hang_theo_id;
CREATE PROCEDURE proc_lay_khach_hang_theo_id(
    IN p_ma_khach_hang VARCHAR(20)
)
BEGIN
    SELECT 
        kh.ma_khach_hang,
        kh.ten_khach_hang,
        kh.so_dien_thoai,
        kh.email,
        kh.dia_chi,
        (SELECT COUNT(*) FROM ve WHERE ma_khach_hang = kh.ma_khach_hang) AS tong_ve_da_dat,
        (SELECT COUNT(*) FROM ve WHERE ma_khach_hang = kh.ma_khach_hang AND trang_thai = 'Completed') AS tong_ve_hoan_thanh
    FROM khach_hang kh
    WHERE kh.ma_khach_hang = p_ma_khach_hang;
END;

-- proc_tao_khach_hang
DROP PROCEDURE IF EXISTS proc_tao_khach_hang;
CREATE PROCEDURE proc_tao_khach_hang(
    IN p_ma_khach_hang VARCHAR(20),
    IN p_ten_khach_hang VARCHAR(100),
    IN p_so_dien_thoai VARCHAR(15),
    IN p_email VARCHAR(100),
    IN p_dia_chi VARCHAR(200)
)
BEGIN
    IF EXISTS (SELECT 1 FROM khach_hang WHERE so_dien_thoai = p_so_dien_thoai) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'So dien thoai da duoc dang ky';
    END IF;

    IF p_email IS NOT NULL AND EXISTS (SELECT 1 FROM khach_hang WHERE email = p_email) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Email da duoc dang ky';
    END IF;

    INSERT INTO khach_hang (
        ma_khach_hang, ten_khach_hang, so_dien_thoai, email, dia_chi
    )
    VALUES (
        p_ma_khach_hang, p_ten_khach_hang, p_so_dien_thoai, p_email, p_dia_chi
    );

    SELECT ROW_COUNT() AS affected_rows;
END;

-- proc_cap_nhat_khach_hang
DROP PROCEDURE IF EXISTS proc_cap_nhat_khach_hang;
CREATE PROCEDURE proc_cap_nhat_khach_hang(
    IN p_ma_khach_hang VARCHAR(20),
    IN p_ten_khach_hang VARCHAR(100),
    IN p_so_dien_thoai VARCHAR(15),
    IN p_email VARCHAR(100),
    IN p_dia_chi VARCHAR(200)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM khach_hang WHERE ma_khach_hang = p_ma_khach_hang) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Khach hang khong ton tai';
    END IF;

    IF p_so_dien_thoai IS NOT NULL AND EXISTS (
        SELECT 1 FROM khach_hang WHERE so_dien_thoai = p_so_dien_thoai AND ma_khach_hang != p_ma_khach_hang
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'So dien thoai da duoc su dung boi khach hang khac';
    END IF;

    IF p_email IS NOT NULL AND EXISTS (
        SELECT 1 FROM khach_hang WHERE email = p_email AND ma_khach_hang != p_ma_khach_hang
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Email da duoc su dung boi khach hang khac';
    END IF;

    UPDATE khach_hang
    SET 
        ten_khach_hang = COALESCE(p_ten_khach_hang, ten_khach_hang),
        so_dien_thoai = COALESCE(p_so_dien_thoai, so_dien_thoai),
        email = COALESCE(p_email, email),
        dia_chi = COALESCE(p_dia_chi, dia_chi)
    WHERE ma_khach_hang = p_ma_khach_hang;

    SELECT ROW_COUNT() AS affected_rows;
END;

-- =============================================================================
-- VERIFY INSTALLATION
-- =============================================================================
SELECT 
    ROUTINE_NAME AS 'Stored Procedure',
    ROUTINE_TYPE AS 'Type',
    CREATED AS 'Created'
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_SCHEMA = DATABASE()
    AND ROUTINE_NAME LIKE 'proc_%'
ORDER BY ROUTINE_NAME;
