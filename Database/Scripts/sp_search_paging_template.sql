-- =============================================================================
-- HIGH-PERFORMANCE SEARCH & PAGING STORED PROCEDURE TEMPLATE
-- =============================================================================
-- 
-- Author: Senior .NET Backend Architect
-- Purpose: Gold Standard template for all entity search operations
-- Pattern: Universal Search with Dynamic Sorting and Efficient Paging
--
-- PERFORMANCE NOTES:
-- =============================================================================
-- 
-- 1. INDEXING STRATEGY (Critical for Performance)
-- ------------------------------------------------
-- 
-- For this SP to perform well, you MUST create these indexes:
--
-- a) Composite Index for Filtering + Sorting:
--    CREATE INDEX idx_xe_status_bien_so ON xe(trang_thai, bien_so);
--    CREATE INDEX idx_xe_hang_sx_trang_thai ON xe(hang_san_xuat, trang_thai);
--
-- b) Full-Text Index for Keyword Search (if supported):
--    CREATE FULLTEXT INDEX idx_xe_fulltext ON xe(ten_xe, bien_so);
--
-- c) Covering Index for Common Queries:
--    CREATE INDEX idx_xe_search_cover ON xe(trang_thai, hang_san_xuat, bien_so, ten_xe);
--
-- 2. WHY NOT DYNAMIC SQL?
-- ------------------------------------------------
-- Dynamic SQL (PREPARE/EXECUTE) allows true dynamic ORDER BY but:
-- - Prevents query plan caching
-- - Higher risk of SQL injection if not careful
-- - More complex to maintain
--
-- The CASE-based ORDER BY approach:
-- - Query plan is cached
-- - No injection risk
-- - Slightly less flexible but much safer
--
-- =============================================================================

DELIMITER //

-- =============================================================================
-- sp_bus_search_paging - Search vehicles with paging and dynamic sorting
-- =============================================================================
-- 
-- Features:
-- - Keyword search across multiple columns (ten_xe, bien_so)
-- - Status and Manufacturer filtering
-- - Dynamic sorting by multiple columns
-- - Efficient paging with LIMIT/OFFSET
-- - Total count via OUTPUT parameter
--
-- Parameters:
-- - p_keyword: Search term (NULL = no filter)
-- - p_status: Vehicle status filter (NULL = no filter)
-- - p_hang_san_xuat: Manufacturer filter (NULL = no filter)
-- - p_bus_type: Bus type filter (NULL = no filter)
-- - p_offset: Skip N records (calculated: (pageIndex-1) * pageSize)
-- - p_limit: Take N records (pageSize)
-- - p_sort_by: Column to sort ('bien_so', 'ten_xe', 'total_trips', etc.)
-- - p_sort_desc: 1 = DESC, 0 = ASC
-- - p_date_from: Filter by registration date from
-- - p_date_to: Filter by registration date to
-- - p_total_record: OUTPUT - Total matching records
--
DROP PROCEDURE IF EXISTS sp_bus_search_paging //
CREATE PROCEDURE sp_bus_search_paging(
    IN p_keyword VARCHAR(200),
    IN p_status VARCHAR(30),
    IN p_hang_san_xuat VARCHAR(50),
    IN p_bus_type VARCHAR(50),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    IN p_date_from DATETIME,
    IN p_date_to DATETIME,
    OUT p_total_record BIGINT
)
BEGIN
    -- =========================================================================
    -- STEP 1: Count total matching records
    -- =========================================================================
    -- This query must use the SAME WHERE clause as the data query
    -- to ensure accurate paging metadata.
    --
    -- PERFORMANCE TIP: If count is slow, consider:
    -- 1. Using SQL_CALC_FOUND_ROWS (deprecated in MySQL 8.0.17+)
    -- 2. Caching counts for common filter combinations
    -- 3. Using approximate counts for very large tables
    -- =========================================================================
    
    SELECT COUNT(*) INTO p_total_record
    FROM xe x
    LEFT JOIN chuyen_xe cx ON cx.ma_xe = x.ma_xe
    WHERE 
        -- Keyword search: Check multiple columns
        -- Pattern: (NULL check OR LIKE pattern)
        -- This allows the optimizer to skip the condition if p_keyword is NULL
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        -- Status filter
        AND (p_status IS NULL OR x.trang_thai = p_status)
        -- Manufacturer filter
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat)
        -- Bus type filter (if you have a bus_type column or join)
        -- AND (p_bus_type IS NULL OR x.loai_xe = p_bus_type)
        -- Date range filter (registration/inspection date)
        AND (p_date_from IS NULL OR x.ngay_dang_kiem >= p_date_from)
        AND (p_date_to IS NULL OR x.ngay_dang_kiem <= p_date_to);

    -- =========================================================================
    -- STEP 2: Return paged data with dynamic sorting
    -- =========================================================================
    -- 
    -- DYNAMIC SORTING with CASE statements:
    -- This approach is SQL-injection safe because:
    -- - Column names are NOT concatenated into SQL
    -- - Sort values are evaluated at runtime
    -- - Invalid sort columns fall back to default
    --
    -- HOW IT WORKS:
    -- ORDER BY 
    --   CASE WHEN @sort = 'col1' AND @desc = 1 THEN col1 END DESC,
    --   CASE WHEN @sort = 'col1' AND @desc = 0 THEN col1 END ASC,
    --   default_column DESC  -- Fallback
    --
    -- =========================================================================
    
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
        -- Calculated fields (joined data)
        COUNT(cx.ma_chuyen) AS total_trips,
        -- Driver info (if assigned) - example of JOIN
        (SELECT tx.ten_tai_xe 
         FROM phan_cong_tai_xe pctx 
         JOIN tai_xe tx ON pctx.ma_tai_xe = tx.ma_tai_xe 
         WHERE pctx.ma_xe = x.ma_xe 
         LIMIT 1) AS driver_name
    FROM xe x
    LEFT JOIN chuyen_xe cx ON cx.ma_xe = x.ma_xe
    WHERE 
        -- Same WHERE clause as COUNT query
        (p_keyword IS NULL OR (
            x.ten_xe LIKE CONCAT('%', p_keyword, '%') OR
            x.bien_so LIKE CONCAT('%', p_keyword, '%') OR
            x.ma_xe LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_status IS NULL OR x.trang_thai = p_status)
        AND (p_hang_san_xuat IS NULL OR x.hang_san_xuat = p_hang_san_xuat)
        AND (p_date_from IS NULL OR x.ngay_dang_kiem >= p_date_from)
        AND (p_date_to IS NULL OR x.ngay_dang_kiem <= p_date_to)
    GROUP BY 
        x.ma_xe, x.ten_xe, x.bien_so, x.hang_san_xuat, 
        x.nam_san_xuat, x.ngay_dang_kiem, x.trang_thai,
        x.muc_tieu_hao, x.phu_thu_phi_van_hanh
    
    -- =========================================================================
    -- DYNAMIC ORDER BY using CASE statements
    -- =========================================================================
    -- Each sortable column needs 2 CASE statements: DESC and ASC
    -- NULL values from non-matching CASEs are handled by ORDER BY (sorted last)
    -- =========================================================================
    ORDER BY
        -- Sort by License Plate (bien_so)
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 1 THEN x.bien_so END DESC,
        CASE WHEN p_sort_by = 'bien_so' AND p_sort_desc = 0 THEN x.bien_so END ASC,
        
        -- Sort by Vehicle Name (ten_xe)
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 1 THEN x.ten_xe END DESC,
        CASE WHEN p_sort_by = 'ten_xe' AND p_sort_desc = 0 THEN x.ten_xe END ASC,
        
        -- Sort by Total Trips (calculated field)
        CASE WHEN p_sort_by = 'total_trips' AND p_sort_desc = 1 THEN COUNT(cx.ma_chuyen) END DESC,
        CASE WHEN p_sort_by = 'total_trips' AND p_sort_desc = 0 THEN COUNT(cx.ma_chuyen) END ASC,
        
        -- Sort by Registration Date (ngay_dang_kiem)
        CASE WHEN p_sort_by = 'ngay_dang_kiem' AND p_sort_desc = 1 THEN x.ngay_dang_kiem END DESC,
        CASE WHEN p_sort_by = 'ngay_dang_kiem' AND p_sort_desc = 0 THEN x.ngay_dang_kiem END ASC,
        
        -- Sort by Year of Manufacture (nam_san_xuat)
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 1 THEN x.nam_san_xuat END DESC,
        CASE WHEN p_sort_by = 'nam_san_xuat' AND p_sort_desc = 0 THEN x.nam_san_xuat END ASC,
        
        -- Default sort: By Vehicle ID (ma_xe) DESC
        -- This ensures consistent ordering when no sort column is specified
        x.ma_xe DESC
    
    -- Paging
    LIMIT p_limit OFFSET p_offset;
    
END //

-- =============================================================================
-- sp_driver_search_paging - Search drivers with paging
-- =============================================================================
DROP PROCEDURE IF EXISTS sp_driver_search_paging //
CREATE PROCEDURE sp_driver_search_paging(
    IN p_keyword VARCHAR(200),
    IN p_gioi_tinh VARCHAR(10),
    IN p_que_quan VARCHAR(100),
    IN p_contract_status VARCHAR(20),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    IN p_date_from DATETIME,
    IN p_date_to DATETIME,
    OUT p_total_record BIGINT
)
BEGIN
    -- Count total matching records
    SELECT COUNT(*) INTO p_total_record
    FROM tai_xe tx
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'))
        AND (p_date_from IS NULL OR tx.ngay_ky_hop_dong >= p_date_from)
        AND (p_date_to IS NULL OR tx.ngay_ky_hop_dong <= p_date_to);

    -- Return paged data
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
        tx.he_so_luong,
        -- Calculated: Total trips driven
        (SELECT COUNT(*) FROM phan_cong_tai_xe pctx WHERE pctx.ma_tai_xe = tx.ma_tai_xe) AS total_assignments,
        -- Current vehicle assignment
        (SELECT x.bien_so 
         FROM phan_cong_tai_xe pctx 
         JOIN xe x ON pctx.ma_xe = x.ma_xe 
         WHERE pctx.ma_tai_xe = tx.ma_tai_xe 
         ORDER BY pctx.ma_phan_cong DESC 
         LIMIT 1) AS current_vehicle
    FROM tai_xe tx
    WHERE 
        (p_keyword IS NULL OR (
            tx.ten_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.ma_tai_xe LIKE CONCAT('%', p_keyword, '%') OR
            tx.so_cccd LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_gioi_tinh IS NULL OR tx.gioi_tinh = p_gioi_tinh)
        AND (p_que_quan IS NULL OR tx.que_quan LIKE CONCAT('%', p_que_quan, '%'))
        AND (p_date_from IS NULL OR tx.ngay_ky_hop_dong >= p_date_from)
        AND (p_date_to IS NULL OR tx.ngay_ky_hop_dong <= p_date_to)
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 1 THEN tx.ten_tai_xe END DESC,
        CASE WHEN p_sort_by = 'ten_tai_xe' AND p_sort_desc = 0 THEN tx.ten_tai_xe END ASC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 1 THEN tx.ngay_ky_hop_dong END DESC,
        CASE WHEN p_sort_by = 'ngay_ky_hop_dong' AND p_sort_desc = 0 THEN tx.ngay_ky_hop_dong END ASC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 1 THEN tx.he_so_luong END DESC,
        CASE WHEN p_sort_by = 'he_so_luong' AND p_sort_desc = 0 THEN tx.he_so_luong END ASC,
        tx.ma_tai_xe DESC
    LIMIT p_limit OFFSET p_offset;
    
END //

-- =============================================================================
-- sp_route_search_paging - Search routes with paging
-- =============================================================================
DROP PROCEDURE IF EXISTS sp_route_search_paging //
CREATE PROCEDURE sp_route_search_paging(
    IN p_keyword VARCHAR(200),
    IN p_diem_di VARCHAR(100),
    IN p_diem_den VARCHAR(100),
    IN p_ma_do_phuc_tap VARCHAR(20),
    IN p_offset INT,
    IN p_limit INT,
    IN p_sort_by VARCHAR(50),
    IN p_sort_desc INT,
    IN p_date_from DATETIME,
    IN p_date_to DATETIME,
    OUT p_total_record BIGINT
)
BEGIN
    -- Count total
    SELECT COUNT(*) INTO p_total_record
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'))
        AND (p_ma_do_phuc_tap IS NULL OR td.ma_do_phuc_tap = p_ma_do_phuc_tap);

    -- Return data
    SELECT 
        td.ma_tuyen,
        td.ten_tuyen,
        td.diem_di,
        td.diem_den,
        td.khoang_cach,
        td.ma_do_phuc_tap,
        -- Calculated: Total trips on this route
        (SELECT COUNT(*) FROM chuyen_xe cx WHERE cx.ma_tuyen = td.ma_tuyen) AS total_trips,
        -- Calculated: Revenue (if applicable)
        (SELECT COALESCE(SUM(g.gia), 0) 
         FROM chuyen_xe cx 
         JOIN ve v ON v.ma_chuyen = cx.ma_chuyen 
         JOIN gia g ON g.ma_tuyen = td.ma_tuyen 
         WHERE cx.ma_tuyen = td.ma_tuyen) AS total_revenue
    FROM tuyen_duong td
    WHERE 
        (p_keyword IS NULL OR (
            td.ten_tuyen LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_di LIKE CONCAT('%', p_keyword, '%') OR
            td.diem_den LIKE CONCAT('%', p_keyword, '%')
        ))
        AND (p_diem_di IS NULL OR td.diem_di LIKE CONCAT('%', p_diem_di, '%'))
        AND (p_diem_den IS NULL OR td.diem_den LIKE CONCAT('%', p_diem_den, '%'))
        AND (p_ma_do_phuc_tap IS NULL OR td.ma_do_phuc_tap = p_ma_do_phuc_tap)
    ORDER BY
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 1 THEN td.ten_tuyen END DESC,
        CASE WHEN p_sort_by = 'ten_tuyen' AND p_sort_desc = 0 THEN td.ten_tuyen END ASC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 1 THEN td.khoang_cach END DESC,
        CASE WHEN p_sort_by = 'khoang_cach' AND p_sort_desc = 0 THEN td.khoang_cach END ASC,
        td.ma_tuyen DESC
    LIMIT p_limit OFFSET p_offset;
    
END //

DELIMITER ;

-- =============================================================================
-- REQUIRED INDEXES FOR OPTIMAL PERFORMANCE
-- =============================================================================
-- 
-- Execute these after creating the stored procedures:
--
-- For xe (vehicles) table:
-- CREATE INDEX idx_xe_trang_thai ON xe(trang_thai);
-- CREATE INDEX idx_xe_hang_san_xuat ON xe(hang_san_xuat);
-- CREATE INDEX idx_xe_ngay_dang_kiem ON xe(ngay_dang_kiem);
-- CREATE INDEX idx_xe_search ON xe(trang_thai, hang_san_xuat, bien_so);
--
-- For tai_xe (drivers) table:
-- CREATE INDEX idx_tai_xe_gioi_tinh ON tai_xe(gioi_tinh);
-- CREATE INDEX idx_tai_xe_que_quan ON tai_xe(que_quan);
-- CREATE INDEX idx_tai_xe_ngay_ky ON tai_xe(ngay_ky_hop_dong);
-- CREATE INDEX idx_tai_xe_search ON tai_xe(gioi_tinh, que_quan);
--
-- For tuyen_duong (routes) table:
-- CREATE INDEX idx_tuyen_duong_diem_di ON tuyen_duong(diem_di);
-- CREATE INDEX idx_tuyen_duong_diem_den ON tuyen_duong(diem_den);
-- CREATE INDEX idx_tuyen_duong_search ON tuyen_duong(diem_di, diem_den, ma_do_phuc_tap);
--
-- =============================================================================

-- =============================================================================
-- INSERT PROCEDURE CONFIGURATION
-- =============================================================================
-- Add these entries to Sys_ProcedureConfig table:
--
-- INSERT INTO sys_procedure_config (function_key, sp_name, description, is_active)
-- VALUES 
-- ('BUS_SEARCH', 'sp_bus_search_paging', 'Search vehicles with paging', 1),
-- ('DRIVER_SEARCH', 'sp_driver_search_paging', 'Search drivers with paging', 1),
-- ('ROUTE_SEARCH', 'sp_route_search_paging', 'Search routes with paging', 1);
-- =============================================================================
