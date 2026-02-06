-- =============================================================================
-- proc_tinh_luong_thang_tai_xe - Tính lýõng tháng tài x?
-- =============================================================================
-- Module: REPORT
-- Function Key: FunctionKeys.Report.LUONG_THANG_TAI_XE
-- Parameters: p_tu (DATETIME), p_den (DATETIME)
-- Returns: ma_tai_xe, ten_tai_xe, tong_km, so_chuyen, luong_co_ban, phu_cap, luong_thang
-- =============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS proc_tinh_luong_thang_tai_xe //

CREATE PROCEDURE proc_tinh_luong_thang_tai_xe(
    IN p_tu DATETIME,
    IN p_den DATETIME
)
BEGIN
    -- Lýõng cõ b?n gi? ð?nh: 10,000,000 VNÐ
    -- Ph? c?p theo km: 5,000 VNÐ/km
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
END //

DELIMITER ;
