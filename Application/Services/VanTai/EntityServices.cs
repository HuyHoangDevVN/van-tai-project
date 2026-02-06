using Application.DTOs.VanTai;
using Application.Models;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Service implementation for TaiXe (Driver) entity.
/// Uses raw SQL queries via ISqlExecuteService + Dynamic SP for Search.
/// </summary>
public class TaiXeService : BaseService, ITaiXeService
{
    public TaiXeService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<TaiXeService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<DriverSearchResultDto>>> SearchAsync(
        DriverSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching drivers - Keyword: {Keyword}, Page: {Page}, Size: {Size}",
            request.NormalizedKeyword, request.NormalizedPageIndex, request.NormalizedPageSize);

        // Validate request
        var validation = ValidateSearchRequest<TPaging<DriverSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            // Build entity-specific filter parameters
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_gioi_tinh", NullIfEmpty(request.GioiTinh)),
                SqlParamModel.Input("p_que_quan", NullIfEmpty(request.QueQuan)),
                SqlParamModel.Input("p_contract_status", NullIfEmpty(request.ContractStatus))
            };

            // Execute paged search using base helper
            return await ExecutePagedSearchAsync<DriverSearchResultDto>(
                FunctionKeys.Driver.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<DriverSearchResultDto>>(ex, "SearchDrivers");
        }
    }

    public async Task<BaseResponse<List<TaiXeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Getting all drivers");

        try
        {
            // Using raw SQL query since no SP exists for this
            var sqlModel = SqlExecuteModel.RawQuery(
                "SELECT * FROM tai_xe ORDER BY ten_tai_xe");

            return await SqlService.ExecuteSqlRawCommandAsync<TaiXeDto>(sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting all drivers");
            return BaseResponse<List<TaiXeDto>>.Error(SqlConstants.Message_Error);
        }
    }

    public async Task<BaseResponse<TaiXeDto?>> GetByIdAsync(string maTaiXe, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Getting driver by ID: {MaTaiXe}", maTaiXe);

        try
        {
            var sqlModel = SqlExecuteModel.RawQuery(
                "SELECT * FROM tai_xe WHERE ma_tai_xe = @ma_tai_xe")
                .AddInput("ma_tai_xe", maTaiXe);

            var result = await SqlService.ExecuteSqlRawCommandAsync<TaiXeDto>(sqlModel, cancellationToken);

            if (result.Success)
            {
                var item = result.Data?.FirstOrDefault();
                return item != null
                    ? BaseResponse<TaiXeDto?>.Ok(item)
                    : BaseResponse<TaiXeDto?>.NotFound($"Không tìm thấy tài xế với mã {maTaiXe}");
            }

            return BaseResponse<TaiXeDto?>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting driver {MaTaiXe}", maTaiXe);
            return BaseResponse<TaiXeDto?>.Error(SqlConstants.Message_Error);
        }
    }

    public async Task<BaseResponse<object>> CreateAsync(TaiXeRequest request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Creating driver: {MaTaiXe}", request.MaTaiXe);

        try
        {
            var sqlModel = SqlExecuteModel.RawQuery(@"
                INSERT INTO tai_xe (ma_tai_xe, ten_tai_xe, ngay_sinh, gioi_tinh, que_quan, 
                    ton_giao, so_cccd, ngay_ky_hop_dong, tuoi, he_so_luong)
                VALUES (@ma_tai_xe, @ten_tai_xe, @ngay_sinh, @gioi_tinh, @que_quan,
                    @ton_giao, @so_cccd, @ngay_ky_hop_dong, @tuoi, @he_so_luong)")
                .AddInput("ma_tai_xe", request.MaTaiXe)
                .AddInput("ten_tai_xe", request.TenTaiXe)
                .AddInput("ngay_sinh", request.NgaySinh)
                .AddInput("gioi_tinh", request.GioiTinh)
                .AddInput("que_quan", request.QueQuan)
                .AddInput("ton_giao", request.TonGiao)
                .AddInput("so_cccd", request.SoCccd)
                .AddInput("ngay_ky_hop_dong", request.NgayKyHopDong)
                .AddInput("tuoi", request.Tuoi)
                .AddInput("he_so_luong", request.HeSoLuong);

            var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

            return result.Success
                ? BaseResponse<object>.Ok("Thêm tài xế thành công")
                : BaseResponse<object>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating driver {MaTaiXe}", request.MaTaiXe);
            return BaseResponse<object>.Error(SqlConstants.Message_Error);
        }
    }

    public async Task<BaseResponse<object>> UpdateAsync(string maTaiXe, TaiXeRequest request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating driver: {MaTaiXe}", maTaiXe);

        try
        {
            var sqlModel = SqlExecuteModel.RawQuery(@"
                UPDATE tai_xe SET 
                    ten_tai_xe = @ten_tai_xe,
                    ngay_sinh = @ngay_sinh,
                    gioi_tinh = @gioi_tinh,
                    que_quan = @que_quan,
                    ton_giao = @ton_giao,
                    so_cccd = @so_cccd,
                    ngay_ky_hop_dong = @ngay_ky_hop_dong,
                    tuoi = @tuoi,
                    he_so_luong = @he_so_luong
                WHERE ma_tai_xe = @ma_tai_xe")
                .AddInput("ma_tai_xe", maTaiXe)
                .AddInput("ten_tai_xe", request.TenTaiXe)
                .AddInput("ngay_sinh", request.NgaySinh)
                .AddInput("gioi_tinh", request.GioiTinh)
                .AddInput("que_quan", request.QueQuan)
                .AddInput("ton_giao", request.TonGiao)
                .AddInput("so_cccd", request.SoCccd)
                .AddInput("ngay_ky_hop_dong", request.NgayKyHopDong)
                .AddInput("tuoi", request.Tuoi)
                .AddInput("he_so_luong", request.HeSoLuong);

            var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

            if (result.Success && result.Data == 0)
            {
                return BaseResponse<object>.NotFound($"Không tìm thấy tài xế với mã {maTaiXe}");
            }

            return result.Success
                ? BaseResponse<object>.Ok("Cập nhật tài xế thành công")
                : BaseResponse<object>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating driver {MaTaiXe}", maTaiXe);
            return BaseResponse<object>.Error(SqlConstants.Message_Error);
        }
    }

    public async Task<BaseResponse<object>> DeleteAsync(string maTaiXe, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Deleting driver: {MaTaiXe}", maTaiXe);

        try
        {
            var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM tai_xe WHERE ma_tai_xe = @ma_tai_xe")
                .AddInput("ma_tai_xe", maTaiXe);

            var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

            if (result.Success && result.Data == 0)
            {
                return BaseResponse<object>.NotFound($"Không tìm thấy tài xế với mã {maTaiXe}");
            }

            return result.Success
                ? BaseResponse<object>.Ok("Xóa tài xế thành công")
                : BaseResponse<object>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting driver {MaTaiXe}", maTaiXe);
            return BaseResponse<object>.Error(SqlConstants.Message_Error);
        }
    }
}

/// <summary>
/// Service implementation for Xe (Vehicle) entity.
/// Uses BaseService for paged search operations.
/// </summary>
public class XeService : BaseService, IXeService
{
    public XeService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<XeService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<BusSearchResultDto>>> SearchAsync(
        BusSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching buses - Keyword: {Keyword}, Status: {Status}, Page: {Page}",
            request.NormalizedKeyword, request.Status, request.NormalizedPageIndex);

        // Validate request
        var validation = ValidateSearchRequest<TPaging<BusSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            // Build entity-specific filter parameters
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_status", NullIfEmpty(request.Status)),
                SqlParamModel.Input("p_hang_san_xuat", NullIfEmpty(request.HangSanXuat)),
                SqlParamModel.Input("p_bus_type", NullIfEmpty(request.BusType))
            };

            // Execute paged search using base helper
            return await ExecutePagedSearchAsync<BusSearchResultDto>(
                FunctionKeys.Bus.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<BusSearchResultDto>>(ex, "SearchBuses");
        }
    }

    public async Task<BaseResponse<List<XeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM xe ORDER BY ten_xe");
        return await SqlService.ExecuteSqlRawCommandAsync<XeDto>(sqlModel, cancellationToken);
    }

    public async Task<BaseResponse<XeDto?>> GetByIdAsync(string maXe, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM xe WHERE ma_xe = @ma_xe")
            .AddInput("ma_xe", maXe);

        var result = await SqlService.ExecuteSqlRawCommandAsync<XeDto>(sqlModel, cancellationToken);

        if (result.Success)
        {
            var item = result.Data?.FirstOrDefault();
            return item != null
                ? BaseResponse<XeDto?>.Ok(item)
                : BaseResponse<XeDto?>.NotFound($"Không tìm thấy xe với mã {maXe}");
        }

        return BaseResponse<XeDto?>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> CreateAsync(XeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            INSERT INTO xe (ma_xe, ten_xe, bien_so, hang_san_xuat, nam_san_xuat,
                ngay_dang_kiem, trang_thai, muc_tieu_hao, phu_thu_phi_van_hanh)
            VALUES (@ma_xe, @ten_xe, @bien_so, @hang_san_xuat, @nam_san_xuat,
                @ngay_dang_kiem, @trang_thai, @muc_tieu_hao, @phu_thu_phi_van_hanh)")
            .AddInput("ma_xe", request.MaXe)
            .AddInput("ten_xe", request.TenXe)
            .AddInput("bien_so", request.BienSo)
            .AddInput("hang_san_xuat", request.HangSanXuat)
            .AddInput("nam_san_xuat", request.NamSanXuat)
            .AddInput("ngay_dang_kiem", request.NgayDangKiem)
            .AddInput("trang_thai", request.TrangThai)
            .AddInput("muc_tieu_hao", request.MucTieuHao)
            .AddInput("phu_thu_phi_van_hanh", request.PhuThuPhiVanHanh);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);
        return result.Success
            ? BaseResponse<object>.Ok("Thêm xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> UpdateAsync(string maXe, XeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            UPDATE xe SET 
                ten_xe = @ten_xe, bien_so = @bien_so, hang_san_xuat = @hang_san_xuat,
                nam_san_xuat = @nam_san_xuat, ngay_dang_kiem = @ngay_dang_kiem,
                trang_thai = @trang_thai, muc_tieu_hao = @muc_tieu_hao,
                phu_thu_phi_van_hanh = @phu_thu_phi_van_hanh
            WHERE ma_xe = @ma_xe")
            .AddInput("ma_xe", maXe)
            .AddInput("ten_xe", request.TenXe)
            .AddInput("bien_so", request.BienSo)
            .AddInput("hang_san_xuat", request.HangSanXuat)
            .AddInput("nam_san_xuat", request.NamSanXuat)
            .AddInput("ngay_dang_kiem", request.NgayDangKiem)
            .AddInput("trang_thai", request.TrangThai)
            .AddInput("muc_tieu_hao", request.MucTieuHao)
            .AddInput("phu_thu_phi_van_hanh", request.PhuThuPhiVanHanh);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy xe với mã {maXe}");

        return result.Success
            ? BaseResponse<object>.Ok("Cập nhật xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> DeleteAsync(string maXe, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM xe WHERE ma_xe = @ma_xe")
            .AddInput("ma_xe", maXe);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy xe với mã {maXe}");

        return result.Success
            ? BaseResponse<object>.Ok("Xóa xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }
}

/// <summary>
/// Service implementation for ChuyenXe (Trip) entity.
/// Uses BaseService for paged search operations.
/// </summary>
public class ChuyenXeService : BaseService, IChuyenXeService
{
    public ChuyenXeService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<ChuyenXeService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<TripSearchResultDto>>> SearchAsync(
        TripSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching trips - Keyword: {Keyword}, Route: {Route}, Page: {Page}",
            request.NormalizedKeyword, request.MaTuyen, request.NormalizedPageIndex);

        var validation = ValidateSearchRequest<TPaging<TripSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_ma_tuyen", NullIfEmpty(request.MaTuyen)),
                SqlParamModel.Input("p_ma_xe", NullIfEmpty(request.MaXe)),
                SqlParamModel.Input("p_status", NullIfEmpty(request.Status))
            };

            return await ExecutePagedSearchAsync<TripSearchResultDto>(
                FunctionKeys.Trip.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<TripSearchResultDto>>(ex, "SearchTrips");
        }
    }

    public async Task<BaseResponse<List<ChuyenXeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM chuyen_xe ORDER BY thoi_gian_khoi_hanh DESC");
        return await SqlService.ExecuteSqlRawCommandAsync<ChuyenXeDto>(sqlModel, cancellationToken);
    }

    public async Task<BaseResponse<ChuyenXeDto?>> GetByIdAsync(string maChuyen, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM chuyen_xe WHERE ma_chuyen = @ma_chuyen")
            .AddInput("ma_chuyen", maChuyen);

        var result = await SqlService.ExecuteSqlRawCommandAsync<ChuyenXeDto>(sqlModel, cancellationToken);

        if (result.Success)
        {
            var item = result.Data?.FirstOrDefault();
            return item != null
                ? BaseResponse<ChuyenXeDto?>.Ok(item)
                : BaseResponse<ChuyenXeDto?>.NotFound($"Không tìm thấy chuyến xe với mã {maChuyen}");
        }

        return BaseResponse<ChuyenXeDto?>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> CreateAsync(ChuyenXeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            INSERT INTO chuyen_xe (ma_chuyen, ten_chuyen, thoi_gian_khoi_hanh, thoi_gian_den, ma_xe, ma_tuyen)
            VALUES (@ma_chuyen, @ten_chuyen, @thoi_gian_khoi_hanh, @thoi_gian_den, @ma_xe, @ma_tuyen)")
            .AddInput("ma_chuyen", request.MaChuyen)
            .AddInput("ten_chuyen", request.TenChuyen)
            .AddInput("thoi_gian_khoi_hanh", request.ThoiGianKhoiHanh)
            .AddInput("thoi_gian_den", request.ThoiGianDen)
            .AddInput("ma_xe", request.MaXe)
            .AddInput("ma_tuyen", request.MaTuyen);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);
        return result.Success
            ? BaseResponse<object>.Ok("Thêm chuyến xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> UpdateAsync(string maChuyen, ChuyenXeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            UPDATE chuyen_xe SET 
                ten_chuyen = @ten_chuyen, thoi_gian_khoi_hanh = @thoi_gian_khoi_hanh,
                thoi_gian_den = @thoi_gian_den, ma_xe = @ma_xe, ma_tuyen = @ma_tuyen
            WHERE ma_chuyen = @ma_chuyen")
            .AddInput("ma_chuyen", maChuyen)
            .AddInput("ten_chuyen", request.TenChuyen)
            .AddInput("thoi_gian_khoi_hanh", request.ThoiGianKhoiHanh)
            .AddInput("thoi_gian_den", request.ThoiGianDen)
            .AddInput("ma_xe", request.MaXe)
            .AddInput("ma_tuyen", request.MaTuyen);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy chuyến xe với mã {maChuyen}");

        return result.Success
            ? BaseResponse<object>.Ok("Cập nhật chuyến xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> DeleteAsync(string maChuyen, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM chuyen_xe WHERE ma_chuyen = @ma_chuyen")
            .AddInput("ma_chuyen", maChuyen);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy chuyến xe với mã {maChuyen}");

        return result.Success
            ? BaseResponse<object>.Ok("Xóa chuyến xe thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }
}

/// <summary>
/// Service implementation for KhachHang (Customer) entity.
/// Uses BaseService for paged search operations.
/// </summary>
public class KhachHangService : BaseService, IKhachHangService
{
    public KhachHangService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<KhachHangService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<CustomerSearchResultDto>>> SearchAsync(
        CustomerSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching customers - Keyword: {Keyword}, Page: {Page}",
            request.NormalizedKeyword, request.NormalizedPageIndex);

        var validation = ValidateSearchRequest<TPaging<CustomerSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_ma_giam_ho", NullIfEmpty(request.MaGiamHo)),
                SqlParamModel.Input("p_customer_type", NullIfEmpty(request.CustomerType))
            };

            return await ExecutePagedSearchAsync<CustomerSearchResultDto>(
                FunctionKeys.Customer.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<CustomerSearchResultDto>>(ex, "SearchCustomers");
        }
    }

    public async Task<BaseResponse<List<KhachHangDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM khach_hang ORDER BY ten");
        return await SqlService.ExecuteSqlRawCommandAsync<KhachHangDto>(sqlModel, cancellationToken);
    }

    public async Task<BaseResponse<KhachHangDto?>> GetByIdAsync(string maKhach, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM khach_hang WHERE ma_khach = @ma_khach")
            .AddInput("ma_khach", maKhach);

        var result = await SqlService.ExecuteSqlRawCommandAsync<KhachHangDto>(sqlModel, cancellationToken);

        if (result.Success)
        {
            var item = result.Data?.FirstOrDefault();
            return item != null
                ? BaseResponse<KhachHangDto?>.Ok(item)
                : BaseResponse<KhachHangDto?>.NotFound($"Không tìm thấy khách hàng với mã {maKhach}");
        }

        return BaseResponse<KhachHangDto?>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> CreateAsync(KhachHangRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            INSERT INTO khach_hang (ma_khach, ten, ngay_sinh, dien_thoai, email, so_cccd, ma_giam_ho)
            VALUES (@ma_khach, @ten, @ngay_sinh, @dien_thoai, @email, @so_cccd, @ma_giam_ho)")
            .AddInput("ma_khach", request.MaKhach)
            .AddInput("ten", request.Ten)
            .AddInput("ngay_sinh", request.NgaySinh)
            .AddInput("dien_thoai", request.DienThoai)
            .AddInput("email", request.Email)
            .AddInput("so_cccd", request.SoCccd)
            .AddInput("ma_giam_ho", request.MaGiamHo);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);
        return result.Success
            ? BaseResponse<object>.Ok("Thêm khách hàng thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> UpdateAsync(string maKhach, KhachHangRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            UPDATE khach_hang SET 
                ten = @ten, ngay_sinh = @ngay_sinh, dien_thoai = @dien_thoai,
                email = @email, so_cccd = @so_cccd, ma_giam_ho = @ma_giam_ho
            WHERE ma_khach = @ma_khach")
            .AddInput("ma_khach", maKhach)
            .AddInput("ten", request.Ten)
            .AddInput("ngay_sinh", request.NgaySinh)
            .AddInput("dien_thoai", request.DienThoai)
            .AddInput("email", request.Email)
            .AddInput("so_cccd", request.SoCccd)
            .AddInput("ma_giam_ho", request.MaGiamHo);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy khách hàng với mã {maKhach}");

        return result.Success
            ? BaseResponse<object>.Ok("Cập nhật khách hàng thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> DeleteAsync(string maKhach, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM khach_hang WHERE ma_khach = @ma_khach")
            .AddInput("ma_khach", maKhach);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy khách hàng với mã {maKhach}");

        return result.Success
            ? BaseResponse<object>.Ok("Xóa khách hàng thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }
}

/// <summary>
/// Service implementation for TuyenDuong (Route) entity.
/// Uses BaseService for paged search operations.
/// </summary>
public class TuyenDuongService : BaseService, ITuyenDuongService
{
    public TuyenDuongService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<TuyenDuongService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<RouteSearchResultDto>>> SearchAsync(
        RouteSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching routes - Keyword: {Keyword}, From: {From}, To: {To}",
            request.NormalizedKeyword, request.DiemDi, request.DiemDen);

        var validation = ValidateSearchRequest<TPaging<RouteSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            // Build entity-specific filter parameters
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_diem_di", NullIfEmpty(request.DiemDi)),
                SqlParamModel.Input("p_diem_den", NullIfEmpty(request.DiemDen)),
                SqlParamModel.Input("p_ma_do_phuc_tap", NullIfEmpty(request.MaDoPhucTap))
            };

            return await ExecutePagedSearchAsync<RouteSearchResultDto>(
                FunctionKeys.Route.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<RouteSearchResultDto>>(ex, "SearchRoutes");
        }
    }

    public async Task<BaseResponse<List<TuyenDuongDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM tuyen_duong ORDER BY ten_tuyen");
        return await SqlService.ExecuteSqlRawCommandAsync<TuyenDuongDto>(sqlModel, cancellationToken);
    }

    public async Task<BaseResponse<TuyenDuongDto?>> GetByIdAsync(string maTuyen, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM tuyen_duong WHERE ma_tuyen = @ma_tuyen")
            .AddInput("ma_tuyen", maTuyen);

        var result = await SqlService.ExecuteSqlRawCommandAsync<TuyenDuongDto>(sqlModel, cancellationToken);

        if (result.Success)
        {
            var item = result.Data?.FirstOrDefault();
            return item != null
                ? BaseResponse<TuyenDuongDto?>.Ok(item)
                : BaseResponse<TuyenDuongDto?>.NotFound($"Không tìm thấy tuyến đường với mã {maTuyen}");
        }

        return BaseResponse<TuyenDuongDto?>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> CreateAsync(TuyenDuongRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            INSERT INTO tuyen_duong (ma_tuyen, diem_di, diem_den, khoang_cach, ten_tuyen, ma_do_phuc_tap)
            VALUES (@ma_tuyen, @diem_di, @diem_den, @khoang_cach, @ten_tuyen, @ma_do_phuc_tap)")
            .AddInput("ma_tuyen", request.MaTuyen)
            .AddInput("diem_di", request.DiemDi)
            .AddInput("diem_den", request.DiemDen)
            .AddInput("khoang_cach", request.KhoangCach)
            .AddInput("ten_tuyen", request.TenTuyen)
            .AddInput("ma_do_phuc_tap", request.MaDoPhucTap);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);
        return result.Success
            ? BaseResponse<object>.Ok("Thêm tuyến đường thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> UpdateAsync(string maTuyen, TuyenDuongRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            UPDATE tuyen_duong SET 
                diem_di = @diem_di, diem_den = @diem_den, khoang_cach = @khoang_cach,
                ten_tuyen = @ten_tuyen, ma_do_phuc_tap = @ma_do_phuc_tap
            WHERE ma_tuyen = @ma_tuyen")
            .AddInput("ma_tuyen", maTuyen)
            .AddInput("diem_di", request.DiemDi)
            .AddInput("diem_den", request.DiemDen)
            .AddInput("khoang_cach", request.KhoangCach)
            .AddInput("ten_tuyen", request.TenTuyen)
            .AddInput("ma_do_phuc_tap", request.MaDoPhucTap);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy tuyến đường với mã {maTuyen}");

        return result.Success
            ? BaseResponse<object>.Ok("Cập nhật tuyến đường thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> DeleteAsync(string maTuyen, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM tuyen_duong WHERE ma_tuyen = @ma_tuyen")
            .AddInput("ma_tuyen", maTuyen);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy tuyến đường với mã {maTuyen}");

        return result.Success
            ? BaseResponse<object>.Ok("Xóa tuyến đường thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }
}

/// <summary>
/// Service implementation for Ve (Ticket) entity.
/// Uses BaseService for paged search operations.
/// </summary>
public class VeService : BaseService, IVeService
{
    public VeService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<VeService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<TicketSearchResultDto>>> SearchAsync(
        TicketSearchReq request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Searching tickets - Keyword: {Keyword}, Customer: {Customer}, Trip: {Trip}",
            request.NormalizedKeyword, request.MaKhach, request.MaChuyen);

        var validation = ValidateSearchRequest<TPaging<TicketSearchResultDto>>(request);
        if (validation != null) return validation;

        try
        {
            var additionalParams = new[]
            {
                SqlParamModel.Input("p_ma_khach", NullIfEmpty(request.MaKhach)),
                SqlParamModel.Input("p_ma_chuyen", NullIfEmpty(request.MaChuyen)),
                SqlParamModel.Input("p_trang_thai_tt", NullIfEmpty(request.TrangThaiTT)),
                SqlParamModel.Input("p_phuong_thuc_tt", NullIfEmpty(request.PhuongThucTT))
            };

            return await ExecutePagedSearchAsync<TicketSearchResultDto>(
                FunctionKeys.Ticket.SEARCH,
                request,
                additionalParams,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<TicketSearchResultDto>>(ex, "SearchTickets");
        }
    }

    public async Task<BaseResponse<List<VeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM ve ORDER BY thoi_gian_dat DESC LIMIT 1000");
        return await SqlService.ExecuteSqlRawCommandAsync<VeDto>(sqlModel, cancellationToken);
    }

    public async Task<BaseResponse<VeDto?>> GetByIdAsync(int stt, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("SELECT * FROM ve WHERE stt = @stt")
            .AddInput("stt", stt);

        var result = await SqlService.ExecuteSqlRawCommandAsync<VeDto>(sqlModel, cancellationToken);

        if (result.Success)
        {
            var item = result.Data?.FirstOrDefault();
            return item != null
                ? BaseResponse<VeDto?>.Ok(item)
                : BaseResponse<VeDto?>.NotFound($"Không tìm thấy vé với STT {stt}");
        }

        return BaseResponse<VeDto?>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> CreateAsync(VeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            INSERT INTO ve (ma_khach, ma_chuyen, phuong_thuc_tt, thoi_gian_dat, vi_tri, trang_thai_tt, ma_ghe, ma_giuong)
            VALUES (@ma_khach, @ma_chuyen, @phuong_thuc_tt, @thoi_gian_dat, @vi_tri, @trang_thai_tt, @ma_ghe, @ma_giuong)")
            .AddInput("ma_khach", request.MaKhach)
            .AddInput("ma_chuyen", request.MaChuyen)
            .AddInput("phuong_thuc_tt", request.PhuongThucTT)
            .AddInput("thoi_gian_dat", request.ThoiGianDat ?? DateTime.Now)
            .AddInput("vi_tri", request.ViTri)
            .AddInput("trang_thai_tt", request.TrangThaiTT)
            .AddInput("ma_ghe", request.MaGhe)
            .AddInput("ma_giuong", request.MaGiuong);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);
        return result.Success
            ? BaseResponse<object>.Ok("Đặt vé thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> UpdateAsync(int stt, VeRequest request, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery(@"
            UPDATE ve SET 
                ma_khach = @ma_khach, ma_chuyen = @ma_chuyen, phuong_thuc_tt = @phuong_thuc_tt,
                vi_tri = @vi_tri, trang_thai_tt = @trang_thai_tt, ma_ghe = @ma_ghe, ma_giuong = @ma_giuong
            WHERE stt = @stt")
            .AddInput("stt", stt)
            .AddInput("ma_khach", request.MaKhach)
            .AddInput("ma_chuyen", request.MaChuyen)
            .AddInput("phuong_thuc_tt", request.PhuongThucTT)
            .AddInput("vi_tri", request.ViTri)
            .AddInput("trang_thai_tt", request.TrangThaiTT)
            .AddInput("ma_ghe", request.MaGhe)
            .AddInput("ma_giuong", request.MaGiuong);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy vé với STT {stt}");

        return result.Success
            ? BaseResponse<object>.Ok("Cập nhật vé thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }

    public async Task<BaseResponse<object>> DeleteAsync(int stt, CancellationToken cancellationToken = default)
    {
        var sqlModel = SqlExecuteModel.RawQuery("DELETE FROM ve WHERE stt = @stt")
            .AddInput("stt", stt);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(sqlModel, cancellationToken);

        if (result.Success && result.Data == 0)
            return BaseResponse<object>.NotFound($"Không tìm thấy vé với STT {stt}");

        return result.Success
            ? BaseResponse<object>.Ok("Xóa vé thành công")
            : BaseResponse<object>.Error(result.Message, result.ErrorCode);
    }
}
