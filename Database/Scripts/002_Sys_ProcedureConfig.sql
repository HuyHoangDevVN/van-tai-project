-- =============================================================================
-- File: 002_Sys_ProcedureConfig.sql
-- Description: Configuration table for Dynamic Stored Procedure Resolution
-- Database: quan_ly_hang_van_tai_hanh_khach_duong_dai
-- =============================================================================

USE quan_ly_hang_van_tai_hanh_khach_duong_dai;

-- =============================================================================
-- TABLE: Sys_ProcedureConfig
-- Purpose: Maps FunctionKey to actual stored procedure names
-- This allows changing SP names without modifying C# code
-- =============================================================================

DROP TABLE IF EXISTS Sys_ProcedureConfig;

CREATE TABLE Sys_ProcedureConfig (
    FunctionKey VARCHAR(100) NOT NULL PRIMARY KEY COMMENT 'Unique key for feature lookup (e.g., BUS_SEARCH)',
    ProcedureName VARCHAR(200) NOT NULL COMMENT 'Actual stored procedure name in MySQL',
    Description VARCHAR(500) NULL COMMENT 'Description of what this SP does',
    ModuleName VARCHAR(100) NULL COMMENT 'Module grouping (e.g., Report, Trip, Ticket)',
    IsActive TINYINT(1) DEFAULT 1 COMMENT 'Whether this mapping is active',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_module (ModuleName),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Dynamic SP configuration mapping';

-- =============================================================================
-- SEED DATA: Procedure Mappings
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Module: REPORT (Báo cáo)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('REPORT_CHI_PHI_CO_BAN', 'proc_chi_phi_co_ban', 'Tính chi phí cơ bản cho tất cả chuyến xe', 'Report'),
('REPORT_DOANH_THU_XE_BUS_NGOI', 'proc_doanh_thu_xe_bus_ngoi_thang', 'Doanh thu xe bus ngồi theo tháng', 'Report'),
('REPORT_GIA_VE_XE_BUS_NGOI', 'proc_gia_ve_xe_bus_ngoi', 'Giá vé xe bus ngồi', 'Report'),
('REPORT_DOANH_THU_TUYEN_DUONG', 'proc_tinh_doanh_thu_tuyen_duong_theo_thang', 'Doanh thu tuyến đường theo tháng', 'Report'),
('REPORT_LUONG_THANG_TAI_XE', 'proc_tinh_luong_thang', 'Tính lương tháng tài xế', 'Report');

-- -----------------------------------------------------------------------------
-- Module: MAINTENANCE (Bảo trì xe)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('MAINTENANCE_STATUS_CHECK', 'sp_maintenance_status_check', 'Kiểm tra trạng thái bảo trì xe (360 ngày / km vận hành)', 'Maintenance'),
('MAINTENANCE_CREATE', 'sp_maintenance_create', 'Thêm lịch bảo trì mới', 'Maintenance'),
('MAINTENANCE_GET_HISTORY', 'sp_maintenance_get_history', 'Lấy lịch sử bảo trì của xe', 'Maintenance'),
('MAINTENANCE_UPDATE', 'sp_maintenance_update', 'Cập nhật thông tin bảo trì', 'Maintenance');

-- -----------------------------------------------------------------------------
-- Module: BUS (Quản lý xe)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('BUS_SEARCH', 'sp_bus_search', 'Tìm kiếm xe theo các tiêu chí', 'Bus'),
('BUS_GET_BY_ID', 'sp_bus_get_by_id', 'Lấy thông tin xe theo mã', 'Bus'),
('BUS_CREATE', 'sp_bus_create', 'Thêm xe mới', 'Bus'),
('BUS_UPDATE', 'sp_bus_update', 'Cập nhật thông tin xe', 'Bus'),
('BUS_DELETE', 'sp_bus_delete', 'Xóa xe', 'Bus'),
('BUS_GET_AVAILABLE', 'sp_bus_get_available', 'Lấy danh sách xe khả dụng cho chuyến', 'Bus');

-- -----------------------------------------------------------------------------
-- Module: DRIVER (Quản lý tài xế)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('DRIVER_SEARCH', 'sp_driver_search', 'Tìm kiếm tài xế', 'Driver'),
('DRIVER_GET_BY_ID', 'sp_driver_get_by_id', 'Lấy thông tin tài xế theo mã', 'Driver'),
('DRIVER_CREATE', 'sp_driver_create', 'Thêm tài xế mới', 'Driver'),
('DRIVER_UPDATE', 'sp_driver_update', 'Cập nhật thông tin tài xế', 'Driver'),
('DRIVER_DELETE', 'sp_driver_delete', 'Xóa tài xế', 'Driver'),
('DRIVER_GET_AVAILABLE', 'sp_driver_get_available', 'Lấy tài xế khả dụng cho chuyến', 'Driver');

-- -----------------------------------------------------------------------------
-- Module: ROUTE (Quản lý tuyến đường)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('ROUTE_SEARCH', 'sp_route_search', 'Tìm kiếm tuyến đường', 'Route'),
('ROUTE_GET_BY_ID', 'sp_route_get_by_id', 'Lấy thông tin tuyến đường theo mã', 'Route'),
('ROUTE_CREATE', 'sp_route_create', 'Thêm tuyến đường mới', 'Route'),
('ROUTE_UPDATE', 'sp_route_update', 'Cập nhật tuyến đường', 'Route'),
('ROUTE_DELETE', 'sp_route_delete', 'Xóa tuyến đường', 'Route');

-- -----------------------------------------------------------------------------
-- Module: TRIP (Quản lý chuyến xe)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('TRIP_SEARCH', 'sp_trip_search', 'Tìm kiếm chuyến xe', 'Trip'),
('TRIP_GET_BY_ID', 'sp_trip_get_by_id', 'Lấy thông tin chuyến xe theo mã', 'Trip'),
('TRIP_CREATE', 'sp_trip_create', 'Tạo chuyến xe mới (kiểm tra xe & tài xế khả dụng)', 'Trip'),
('TRIP_UPDATE', 'sp_trip_update', 'Cập nhật chuyến xe', 'Trip'),
('TRIP_DELETE', 'sp_trip_delete', 'Xóa/hủy chuyến xe', 'Trip'),
('TRIP_COMPLETE', 'sp_trip_complete_update_stats', 'Hoàn thành chuyến & cập nhật km vận hành xe', 'Trip'),
('TRIP_GET_BY_STATUS', 'sp_trip_get_by_status', 'Lấy chuyến xe theo trạng thái', 'Trip');

-- -----------------------------------------------------------------------------
-- Module: TICKET (Quản lý vé)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('TICKET_SEARCH', 'sp_ticket_search', 'Tìm kiếm vé', 'Ticket'),
('TICKET_GET_BY_ID', 'sp_ticket_get_by_id', 'Lấy thông tin vé', 'Ticket'),
('TICKET_BOOK', 'sp_ticket_booking', 'Đặt vé (kiểm tra số ghế trống)', 'Ticket'),
('TICKET_CANCEL', 'sp_ticket_cancel', 'Hủy vé', 'Ticket'),
('TICKET_GET_BY_TRIP', 'sp_ticket_get_by_trip', 'Lấy danh sách vé theo chuyến', 'Ticket'),
('TICKET_UPDATE_STATUS', 'sp_ticket_update_status', 'Cập nhật trạng thái thanh toán vé', 'Ticket');

-- -----------------------------------------------------------------------------
-- Module: CUSTOMER (Quản lý khách hàng)
-- -----------------------------------------------------------------------------
INSERT INTO Sys_ProcedureConfig (FunctionKey, ProcedureName, Description, ModuleName) VALUES
('CUSTOMER_SEARCH', 'sp_customer_search', 'Tìm kiếm khách hàng', 'Customer'),
('CUSTOMER_GET_BY_ID', 'sp_customer_get_by_id', 'Lấy thông tin khách hàng', 'Customer'),
('CUSTOMER_CREATE', 'sp_customer_create', 'Thêm khách hàng mới', 'Customer'),
('CUSTOMER_UPDATE', 'sp_customer_update', 'Cập nhật khách hàng', 'Customer');

-- =============================================================================
-- Verify data
-- =============================================================================
SELECT ModuleName, COUNT(*) as ProcedureCount 
FROM Sys_ProcedureConfig 
GROUP BY ModuleName 
ORDER BY ModuleName;

SELECT * FROM Sys_ProcedureConfig ORDER BY ModuleName, FunctionKey;
