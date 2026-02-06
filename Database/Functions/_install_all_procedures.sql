-- =============================================================================
-- INSTALL ALL STORED PROCEDURES
-- =============================================================================
-- Run this file to install all stored procedures into the database
-- Ch?y file n�y �? c�i �?t t?t c? stored procedures v�o database
-- =============================================================================

-- =============================================================================
-- REPORT MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_chi_phi_co_ban.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_doanh_thu_xe_bus_ngoi.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_doanh_thu_xe_bus_ngoi_thang.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_gia_ve_xe_bus_ngoi.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_doanh_thu_tuyen_duong.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_doanh_thu_tuyen_duong_thang.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_tinh_luong_thang_tai_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_lay_tuyen_duong_xe_giuong_nam.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Report/proc_lay_khach_hang_quay_lai.sql;

-- =============================================================================
-- MAINTENANCE MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Maintenance/proc_kiem_tra_trang_thai_bao_tri.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Maintenance/proc_tao_bao_tri.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Maintenance/proc_lay_lich_su_bao_tri.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Maintenance/proc_cap_nhat_bao_tri.sql;

-- =============================================================================
-- BUS MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_tim_kiem_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_lay_xe_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_tao_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_cap_nhat_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_xoa_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Bus/proc_lay_xe_san_sang.sql;

-- =============================================================================
-- DRIVER MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_tim_kiem_tai_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_lay_tai_xe_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_tao_tai_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_cap_nhat_tai_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_xoa_tai_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Driver/proc_lay_tai_xe_san_sang.sql;

-- =============================================================================
-- ROUTE MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Route/proc_tim_kiem_tuyen_duong.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Route/proc_lay_tuyen_duong_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Route/proc_tao_tuyen_duong.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Route/proc_cap_nhat_tuyen_duong.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Route/proc_xoa_tuyen_duong.sql;

-- =============================================================================
-- TRIP MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_tim_kiem_chuyen_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_lay_chuyen_xe_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_tao_chuyen_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_cap_nhat_chuyen_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_xoa_chuyen_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_hoan_thanh_chuyen_xe.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Trip/proc_lay_chuyen_xe_theo_trang_thai.sql;

-- =============================================================================
-- TICKET MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_tim_kiem_ve.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_lay_ve_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_dat_ve.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_huy_ve.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_lay_ve_theo_chuyen.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Ticket/proc_cap_nhat_trang_thai_ve.sql;

-- =============================================================================
-- CUSTOMER MODULE
-- =============================================================================
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Customer/proc_tim_kiem_khach_hang.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Customer/proc_lay_khach_hang_theo_id.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Customer/proc_tao_khach_hang.sql;
SOURCE D:/Desktop/TaiLieu/ptit/co_so_du_lieu_nc/project/Database/Functions/Customer/proc_cap_nhat_khach_hang.sql;

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
