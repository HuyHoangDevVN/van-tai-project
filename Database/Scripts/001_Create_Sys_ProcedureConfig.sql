-- ============================================================================
-- Script: Create Sys_ProcedureConfig Table
-- Purpose: Stores the mapping between FunctionKey and actual StoredProcedure names
--          Used by ProcedureConfigProvider for dynamic SP resolution
-- ============================================================================

-- Drop table if exists (for development/re-run)
DROP TABLE IF EXISTS Sys_ProcedureConfig;

-- Create the configuration table
CREATE TABLE Sys_ProcedureConfig (
    FunctionKey     VARCHAR(100) NOT NULL PRIMARY KEY COMMENT 'Unique key used in code (e.g., proc_tinh_chi_phi_co_ban)',
    ProcedureName   VARCHAR(100) NOT NULL COMMENT 'Actual stored procedure name in MySQL',
    Description     VARCHAR(500) NULL COMMENT 'Description of what this procedure does',
    ModuleName      VARCHAR(50) NULL COMMENT 'Module/feature group (Report, Maintenance, Bus, etc.)',
    IsActive        BIT DEFAULT 1 COMMENT '1 = Active, 0 = Disabled',
    CreatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create index for common queries
CREATE INDEX IX_Sys_ProcedureConfig_Module ON Sys_ProcedureConfig(ModuleName, IsActive);

-- ============================================================================
-- SEED DATA: Function Keys mapping to Stored Procedures
-- ============================================================================

-- Report Module (Báo cáo)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tinh_chi_phi_co_ban', 'proc_tinh_chi_phi_co_ban', 'Tính chi phí cõ b?n c?a chuy?n xe', 'Report'),
('proc_tinh_doanh_thu_xe_bus_ngoi', 'proc_tinh_doanh_thu_xe_bus_ngoi', 'Tính doanh thu xe bu?t ng?i', 'Report'),
('proc_tinh_doanh_thu_xe_bus_ngoi_thang', 'proc_tinh_doanh_thu_xe_bus_ngoi_thang', 'Tính doanh thu xe bu?t ng?i theo tháng', 'Report'),
('proc_tinh_gia_ve_xe_bus_ngoi', 'proc_tinh_gia_ve_xe_bus_ngoi', 'Tính giá vé xe bu?t ng?i', 'Report'),
('proc_tinh_doanh_thu_tuyen_duong', 'proc_tinh_doanh_thu_tuyen_duong', 'Tính doanh thu theo tuy?n ðý?ng', 'Report'),
('proc_tinh_doanh_thu_tuyen_duong_thang', 'proc_tinh_doanh_thu_tuyen_duong_thang', 'Tính doanh thu tuy?n ðý?ng theo tháng', 'Report'),
('proc_tinh_luong_thang_tai_xe', 'proc_tinh_luong_thang_tai_xe', 'Tính lýõng tháng c?a tài x?', 'Report'),
('proc_lay_tuyen_duong_xe_giuong_nam', 'proc_lay_tuyen_duong_xe_giuong_nam', 'L?y tuy?n ðý?ng có xe giý?ng n?m', 'Report'),
('proc_lay_khach_hang_quay_lai', 'proc_lay_khach_hang_quay_lai', 'L?y danh sách khách hàng quay l?i', 'Report');

-- Maintenance Module (B?o tr?)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_kiem_tra_trang_thai_bao_tri', 'proc_kiem_tra_trang_thai_bao_tri', 'Ki?m tra tr?ng thái b?o tr? xe', 'Maintenance'),
('proc_tao_bao_tri', 'proc_tao_bao_tri', 'T?o l?ch b?o tr? m?i', 'Maintenance'),
('proc_lay_lich_su_bao_tri', 'proc_lay_lich_su_bao_tri', 'L?y l?ch s? b?o tr? xe', 'Maintenance'),
('proc_cap_nhat_bao_tri', 'proc_cap_nhat_bao_tri', 'C?p nh?t thông tin b?o tr?', 'Maintenance');

-- Bus Module (Qu?n l? xe)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_xe', 'proc_tim_kiem_xe', 'T?m ki?m xe theo nhi?u tiêu chí', 'Bus'),
('proc_lay_xe_theo_id', 'proc_lay_xe_theo_id', 'L?y thông tin xe theo m?', 'Bus'),
('proc_tao_xe', 'proc_tao_xe', 'Thêm xe m?i', 'Bus'),
('proc_cap_nhat_xe', 'proc_cap_nhat_xe', 'C?p nh?t thông tin xe', 'Bus'),
('proc_xoa_xe', 'proc_xoa_xe', 'Xóa xe', 'Bus'),
('proc_lay_xe_san_sang', 'proc_lay_xe_san_sang', 'L?y danh sách xe s?n sàng ho?t ð?ng', 'Bus');

-- Driver Module (Qu?n l? tài x?)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_tai_xe', 'proc_tim_kiem_tai_xe', 'T?m ki?m tài x?', 'Driver'),
('proc_lay_tai_xe_theo_id', 'proc_lay_tai_xe_theo_id', 'L?y thông tin tài x? theo m?', 'Driver'),
('proc_tao_tai_xe', 'proc_tao_tai_xe', 'Thêm tài x? m?i', 'Driver'),
('proc_cap_nhat_tai_xe', 'proc_cap_nhat_tai_xe', 'C?p nh?t thông tin tài x?', 'Driver'),
('proc_xoa_tai_xe', 'proc_xoa_tai_xe', 'Xóa tài x?', 'Driver'),
('proc_lay_tai_xe_san_sang', 'proc_lay_tai_xe_san_sang', 'L?y danh sách tài x? s?n sàng', 'Driver');

-- Route Module (Qu?n l? tuy?n ðý?ng)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_tuyen_duong', 'proc_tim_kiem_tuyen_duong', 'T?m ki?m tuy?n ðý?ng', 'Route'),
('proc_lay_tuyen_duong_theo_id', 'proc_lay_tuyen_duong_theo_id', 'L?y thông tin tuy?n ðý?ng theo m?', 'Route'),
('proc_tao_tuyen_duong', 'proc_tao_tuyen_duong', 'Thêm tuy?n ðý?ng m?i', 'Route'),
('proc_cap_nhat_tuyen_duong', 'proc_cap_nhat_tuyen_duong', 'C?p nh?t thông tin tuy?n ðý?ng', 'Route'),
('proc_xoa_tuyen_duong', 'proc_xoa_tuyen_duong', 'Xóa tuy?n ðý?ng', 'Route');

-- Trip Module (Qu?n l? chuy?n xe)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_chuyen_xe', 'proc_tim_kiem_chuyen_xe', 'T?m ki?m chuy?n xe', 'Trip'),
('proc_lay_chuyen_xe_theo_id', 'proc_lay_chuyen_xe_theo_id', 'L?y thông tin chuy?n xe theo m?', 'Trip'),
('proc_tao_chuyen_xe', 'proc_tao_chuyen_xe', 'T?o chuy?n xe m?i', 'Trip'),
('proc_cap_nhat_chuyen_xe', 'proc_cap_nhat_chuyen_xe', 'C?p nh?t thông tin chuy?n xe', 'Trip'),
('proc_xoa_chuyen_xe', 'proc_xoa_chuyen_xe', 'Xóa chuy?n xe', 'Trip'),
('proc_hoan_thanh_chuyen_xe', 'proc_hoan_thanh_chuyen_xe', 'Ðánh d?u chuy?n xe hoàn thành', 'Trip'),
('proc_lay_chuyen_xe_theo_trang_thai', 'proc_lay_chuyen_xe_theo_trang_thai', 'L?y chuy?n xe theo tr?ng thái', 'Trip');

-- Ticket Module (Qu?n l? vé)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_ve', 'proc_tim_kiem_ve', 'T?m ki?m vé', 'Ticket'),
('proc_lay_ve_theo_id', 'proc_lay_ve_theo_id', 'L?y thông tin vé theo ID', 'Ticket'),
('proc_dat_ve', 'proc_dat_ve', 'Ð?t vé m?i', 'Ticket'),
('proc_huy_ve', 'proc_huy_ve', 'H?y vé', 'Ticket'),
('proc_lay_ve_theo_chuyen', 'proc_lay_ve_theo_chuyen', 'L?y danh sách vé theo chuy?n', 'Ticket'),
('proc_cap_nhat_trang_thai_ve', 'proc_cap_nhat_trang_thai_ve', 'C?p nh?t tr?ng thái vé', 'Ticket');

-- Customer Module (Qu?n l? khách hàng)
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('proc_tim_kiem_khach_hang', 'proc_tim_kiem_khach_hang', 'T?m ki?m khách hàng', 'Customer'),
('proc_lay_khach_hang_theo_id', 'proc_lay_khach_hang_theo_id', 'L?y thông tin khách hàng theo m?', 'Customer'),
('proc_tao_khach_hang', 'proc_tao_khach_hang', 'Thêm khách hàng m?i', 'Customer'),
('proc_cap_nhat_khach_hang', 'proc_cap_nhat_khach_hang', 'C?p nh?t thông tin khách hàng', 'Customer');

-- ============================================================================
-- Verification Query
-- ============================================================================
SELECT 
    ModuleName,
    COUNT(*) AS ProcedureCount
FROM Sys_ProcedureConfig
WHERE IsActive = 1
GROUP BY ModuleName
ORDER BY ModuleName;

-- Show all records
SELECT * FROM Sys_ProcedureConfig ORDER BY ModuleName, FunctionKey;
