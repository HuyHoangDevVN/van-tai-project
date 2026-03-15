# API DOCUMENTATION - H? Th?ng Qu?n L? V?n T?i

> **Version:** 1.0  
> **Backend:** .NET 8 Web API  
> **Database:** MySQL  
> **API Style:** RESTful  
> **Auth:** None (hi?n t?i)

---

## 1. T?ng Quan H? Th?ng

### 1.1 M?c ðích
H? th?ng backend API qu?n l? v?n t?i hành khách ðý?ng dài, bao g?m:
- Qu?n l? xe (Bus Management)
- Qu?n l? tài x? (Driver Management)
- Qu?n l? tuy?n ðý?ng (Route Management)
- Qu?n l? chuy?n xe (Trip Management)
- Qu?n l? vé xe (Ticket Management)
- Qu?n l? khách hàng (Customer Management)
- Qu?n l? b?o tr? xe (Maintenance Management)
- Báo cáo th?ng kê (Reports)
- Qu?n l? s?n ph?m (Product Management - demo module)

### 1.2 Các Module Chính

| Module | Route Prefix | Mô t? |
|--------|--------------|-------|
| **Tài x?** | `/api/tai-xe` | CRUD + Search tài x? |
| **Xe** | `/api/xe` | CRUD + Search xe |
| **Chuy?n xe** | `/api/chuyen-xe` | CRUD + Hoàn thành/H?y chuy?n |
| **Khách hàng** | `/api/khach-hang` | CRUD + Search khách hàng |
| **Tuy?n ðý?ng** | `/api/tuyen-duong` | CRUD + Search tuy?n ðý?ng |
| **Vé** | `/api/ve` | CRUD + Ð?t vé/H?y vé |
| **B?o tr?** | `/api/bao-tri` | Qu?n l? b?o tr? xe |
| **Báo cáo** | `/api/baocao` | Các báo cáo th?ng kê |
| **S?n ph?m** | `/api/product` | Demo CRUD s?n ph?m |

---

## 2. Chu?n Chung

### 2.1 Base URL
```
Development: http://localhost:5000
Production: https://your-domain.com
```

### 2.2 Headers M?c Ð?nh
```http
Content-Type: application/json
Accept: application/json
```

### 2.3 Format Response Chu?n

**Success Response:**
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { /* payload */ },
  "errorCode": 0,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errorCode": -1,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 2.4 Error Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `-1` | General Error |
| `-2` | Not Found |
| `-3` | Validation Error |
| `-4` | Duplicate Entry |
| `-5` | Unauthorized |

### 2.5 Paging Response Format
```json
{
  "success": true,
  "data": {
    "items": [ /* array of items */ ],
    "pageIndex": 1,
    "pageSize": 20,
    "totalRecords": 100,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true,
    "currentPageSize": 20,
    "startRecord": 1,
    "endRecord": 20
  }
}
```

---

## 3. Chi Ti?t API

---

### 3.1 TÀI X? (Driver) - `/api/tai-xe`

#### 3.1.1 T?m ki?m tài x?
```http
GET /api/tai-xe/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `keyword` | string | No | T? khóa t?m ki?m (tên, CCCD) |
| `gioiTinh` | string | No | L?c theo gi?i tính: "Nam", "N?" |
| `queQuan` | string | No | L?c theo quê quán |
| `pageIndex` | int | No | Trang hi?n t?i (default: 1) |
| `pageSize` | int | No | S? items/trang (default: 20, max: 100) |
| `sortBy` | string | No | C?t s?p x?p |
| `sortDesc` | bool | No | S?p x?p gi?m d?n (default: true) |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "maTaiXe": "TX001",
        "tenTaiXe": "Nguy?n Vãn A",
        "ngaySinh": "1985-05-15",
        "gioiTinh": "Nam",
        "queQuan": "Hà N?i",
        "soCccd": "001085012345",
        "ngayKyHopDong": "2020-01-01",
        "tuoi": 38,
        "heSoLuong": 1.5,
        "tongSoChuyen": 150,
        "totalAssignments": 200,
        "currentVehicle": "XE001"
      }
    ],
    "pageIndex": 1,
    "pageSize": 20,
    "totalRecords": 50,
    "totalPages": 3
  }
}
```

#### 3.1.2 L?y danh sách t?t c? tài x?
```http
GET /api/tai-xe
```

**Response Success (200):**
```json
{
  "success": true,
  "data": [
    {
      "maTaiXe": "TX001",
      "tenTaiXe": "Nguy?n Vãn A",
      "ngaySinh": "1985-05-15",
      "gioiTinh": "Nam",
      "queQuan": "Hà N?i",
      "tonGiao": "Không",
      "soCccd": "001085012345",
      "ngayKyHopDong": "2020-01-01",
      "tuoi": 38,
      "heSoLuong": 1.5
    }
  ]
}
```

#### 3.1.3 L?y chi ti?t tài x? theo m?
```http
GET /api/tai-xe/{maTaiXe}
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `maTaiXe` | string | M? tài x? |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "maTaiXe": "TX001",
    "tenTaiXe": "Nguy?n Vãn A",
    "ngaySinh": "1985-05-15",
    "gioiTinh": "Nam",
    "queQuan": "Hà N?i",
    "tonGiao": "Không",
    "soCccd": "001085012345",
    "ngayKyHopDong": "2020-01-01",
    "tuoi": 38,
    "heSoLuong": 1.5
  }
}
```

**Response Not Found (404):**
```json
{
  "success": false,
  "message": "Không t?m th?y tài x? v?i m? TX999",
  "errorCode": -2
}
```

#### 3.1.4 Thêm m?i tài x?
```http
POST /api/tai-xe
```

**Request Body:**
```json
{
  "maTaiXe": "TX002",
  "tenTaiXe": "Tr?n Vãn B",
  "ngaySinh": "1990-03-20",
  "gioiTinh": "Nam",
  "queQuan": "TP.HCM",
  "tonGiao": "Ph?t giáo",
  "soCccd": "079090123456",
  "ngayKyHopDong": "2024-01-15",
  "tuoi": 34,
  "heSoLuong": 1.2
}
```

**Validation Rules:**
| Field | Rules |
|-------|-------|
| `maTaiXe` | Required, max 20 chars |
| `tenTaiXe` | Max 100 chars |
| `gioiTinh` | Max 10 chars |
| `queQuan` | Max 100 chars |
| `soCccd` | Max 20 chars |
| `tuoi` | 0-200 |
| `heSoLuong` | 0-100 |

**Response Success (201):**
```json
{
  "success": true,
  "message": "Thêm tài x? thành công"
}
```

#### 3.1.5 C?p nh?t tài x?
```http
PUT /api/tai-xe/{maTaiXe}
```

**Request Body:** (same as create)

**Response Success (200):**
```json
{
  "success": true,
  "message": "C?p nh?t tài x? thành công"
}
```

#### 3.1.6 Xóa tài x?
```http
DELETE /api/tai-xe/{maTaiXe}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa tài x? thành công"
}
```

---

### 3.2 XE (Vehicle) - `/api/xe`

#### 3.2.1 T?m ki?m xe
```http
GET /api/xe/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `keyword` | string | No | T? khóa (tên xe, bi?n s?) |
| `status` | string | No | Tr?ng thái: "Ho?t ð?ng", "B?o tr?", "Ng?ng ho?t ð?ng" |
| `hangSanXuat` | string | No | L?c theo h?ng: "Toyota", "Hyundai"... |
| `pageIndex` | int | No | Trang hi?n t?i |
| `pageSize` | int | No | S? items/trang |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "maXe": "XE001",
        "tenXe": "Xe Bus 45 ch?",
        "bienSo": "30A-12345",
        "hangSanXuat": "Hyundai",
        "namSanXuat": 2020,
        "ngayDangKiem": "2024-06-15",
        "trangThai": "Ho?t ð?ng",
        "tongKmVanHanh": 15000.5,
        "ngayBaoTriCuoi": "2024-01-01",
        "soChoNgoi": 45,
        "totalTrips": 120,
        "driverName": "Nguy?n Vãn A"
      }
    ],
    "totalRecords": 30
  }
}
```

#### 3.2.2 L?y t?t c? xe
```http
GET /api/xe
```

#### 3.2.3 L?y xe theo m?
```http
GET /api/xe/{maXe}
```

#### 3.2.4 Thêm xe m?i
```http
POST /api/xe
```

**Request Body:**
```json
{
  "maXe": "XE002",
  "tenXe": "Xe Giý?ng n?m 40 ch?",
  "bienSo": "30B-54321",
  "hangSanXuat": "Thaco",
  "namSanXuat": 2022,
  "ngayDangKiem": "2024-12-01",
  "trangThai": "Ho?t ð?ng",
  "mucTieuHao": 25.5,
  "phuThuPhiVanHanh": 50000
}
```

#### 3.2.5 C?p nh?t xe
```http
PUT /api/xe/{maXe}
```

#### 3.2.6 Xóa xe
```http
DELETE /api/xe/{maXe}
```

---

### 3.3 CHUY?N XE (Trip) - `/api/chuyen-xe`

#### 3.3.1 T?m ki?m chuy?n xe
```http
GET /api/chuyen-xe/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `keyword` | string | No | T? khóa t?m ki?m |
| `maTuyen` | string | No | L?c theo m? tuy?n |
| `maXe` | string | No | L?c theo m? xe |
| `fromDate` | datetime | No | T? ngày |
| `toDate` | datetime | No | Ð?n ngày |
| `pageIndex` | int | No | Trang hi?n t?i |
| `pageSize` | int | No | S? items/trang |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "maChuyen": "CX001",
        "tenChuyen": "Hà N?i - H?i Ph?ng Sáng",
        "thoiGianKhoiHanh": "2024-01-20T06:00:00",
        "thoiGianDen": "2024-01-20T09:00:00",
        "maXe": "XE001",
        "tenXe": "Xe Bus 45 ch?",
        "bienSo": "30A-12345",
        "maTuyen": "TD001",
        "tenTuyen": "Hà N?i - H?i Ph?ng",
        "trangThai": "Scheduled",
        "soVeDaBan": 30,
        "tongCho": 45
      }
    ]
  }
}
```

#### 3.3.2 Các API CRUD cõ b?n
```http
GET /api/chuyen-xe              # L?y t?t c?
GET /api/chuyen-xe/{maChuyen}   # L?y theo m?
POST /api/chuyen-xe             # Thêm m?i
PUT /api/chuyen-xe/{maChuyen}   # C?p nh?t
DELETE /api/chuyen-xe/{maChuyen} # Xóa
```

**Request Body (Create/Update):**
```json
{
  "maChuyen": "CX002",
  "tenChuyen": "Hà N?i - Ðà N?ng Ðêm",
  "thoiGianKhoiHanh": "2024-01-20T20:00:00",
  "thoiGianDen": "2024-01-21T08:00:00",
  "maXe": "XE002",
  "maTuyen": "TD002"
}
```

#### 3.3.3 ? Hoàn thành chuy?n xe (CRITICAL)
```http
PUT /api/chuyen-xe/{maChuyen}/hoan-thanh
```

**Description:** API quan tr?ng - kích ho?t c?p nh?t d? li?u b?o tr?

**Side Effects:**
1. C?p nh?t tr?ng thái chuy?n ? "Completed"
2. Tính km v?n hành: `KM tãng = Kho?ng cách × H? s? ð? khó`
3. C?p nh?t `xe.tong_km_van_hanh += KM tãng`
4. C?p nh?t `tai_xe.tong_so_chuyen += 1`

**Response Success (200):**
```json
{
  "success": true,
  "message": "Hoàn thành chuy?n xe thành công. D? li?u b?o tr? ð? ðý?c c?p nh?t."
}
```

#### 3.3.4 H?y chuy?n xe
```http
PUT /api/chuyen-xe/{maChuyen}/huy
```

**Note:** Ch? h?y ðý?c chuy?n có tr?ng thái "Scheduled"

**Response Success (200):**
```json
{
  "success": true,
  "message": "H?y chuy?n thành công"
}
```

---

### 3.4 KHÁCH HÀNG (Customer) - `/api/khach-hang`

#### 3.4.1 T?m ki?m khách hàng
```http
GET /api/khach-hang/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `keyword` | string | Tên, SÐT, CCCD |
| `pageIndex` | int | Trang |
| `pageSize` | int | S? items |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "maKhach": "KH001",
        "ten": "Lê Th? C",
        "ngaySinh": "1995-08-20",
        "dienThoai": "0901234567",
        "email": "lethic@email.com",
        "soCccd": "001095012345",
        "maGiamHo": null,
        "totalTickets": 15,
        "totalSpending": 3500000,
        "lastBookingDate": "2024-01-10",
        "guardianName": null
      }
    ]
  }
}
```

#### 3.4.2 CRUD APIs
```http
GET /api/khach-hang
GET /api/khach-hang/{maKhach}
POST /api/khach-hang
PUT /api/khach-hang/{maKhach}
DELETE /api/khach-hang/{maKhach}
```

**Request Body:**
```json
{
  "maKhach": "KH002",
  "ten": "Ph?m Vãn D",
  "ngaySinh": "2000-12-25",
  "dienThoai": "0912345678",
  "email": "phamvand@email.com",
  "soCccd": "001100012345",
  "maGiamHo": null
}
```

---

### 3.5 TUY?N ÐÝ?NG (Route) - `/api/tuyen-duong`

#### 3.5.1 T?m ki?m tuy?n ðý?ng
```http
GET /api/tuyen-duong/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `keyword` | string | Tên tuy?n |
| `diemDi` | string | Ði?m kh?i hành |
| `diemDen` | string | Ði?m ð?n |
| `pageIndex` | int | Trang |
| `pageSize` | int | S? items |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "maTuyen": "TD001",
        "tenTuyen": "Hà N?i - H?i Ph?ng",
        "diemDi": "Hà N?i",
        "diemDen": "H?i Ph?ng",
        "khoangCach": 120.5,
        "maDoPhucTap": "DPT01",
        "tenDoPhucTap": "D?",
        "totalTrips": 500,
        "totalRevenue": 250000000
      }
    ]
  }
}
```

#### 3.5.2 CRUD APIs
```http
GET /api/tuyen-duong
GET /api/tuyen-duong/{maTuyen}
POST /api/tuyen-duong
PUT /api/tuyen-duong/{maTuyen}
DELETE /api/tuyen-duong/{maTuyen}
```

---

### 3.6 VÉ (Ticket) - `/api/ve`

#### 3.6.1 T?m ki?m vé
```http
GET /api/ve/tim-kiem
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `keyword` | string | T? khóa |
| `maKhach` | string | M? khách hàng |
| `maChuyen` | string | M? chuy?n |
| `trangThaiTT` | string | Tr?ng thái thanh toán |
| `pageIndex` | int | Trang |
| `pageSize` | int | S? items |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "stt": 1,
        "maKhach": "KH001",
        "maChuyen": "CX001",
        "phuongThucTT": "Ti?n m?t",
        "thoiGianDat": "2024-01-15T14:30:00",
        "viTri": "A1",
        "trangThaiTT": "Ð? thanh toán",
        "maGhe": 1,
        "maGiuong": null,
        "tenKhach": "Lê Th? C",
        "dienThoai": "0901234567",
        "thoiGianKhoiHanh": "2024-01-20T06:00:00",
        "tenTuyen": "Hà N?i - H?i Ph?ng",
        "giaVe": 150000
      }
    ]
  }
}
```

#### 3.6.2 CRUD APIs cõ b?n
```http
GET /api/ve
GET /api/ve/{stt}
POST /api/ve
PUT /api/ve/{stt}
DELETE /api/ve/{stt}
```

#### 3.6.3 ? Ð?t vé
```http
POST /api/ve/dat-ve
```

**Request Body:**
```json
{
  "maKhach": "KH001",
  "maChuyen": "CX001",
  "phuongThucTT": "Ti?n m?t",
  "viTri": "A1",
  "maGhe": 5,
  "maGiuong": null
}
```

**Validation (Server-side SP):**
- Chuy?n xe t?n t?i và ? tr?ng thái "Scheduled"
- S? vé ð? bán < S? gh? c?a xe
- Gh?/giý?ng chýa ðý?c ð?t

**Response Success (200):**
```json
{
  "success": true,
  "message": "Ð?t vé thành công",
  "data": 123
}
```

#### 3.6.4 H?y vé
```http
PUT /api/ve/{stt}/huy
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "H?y vé thành công"
}
```

#### 3.6.5 L?y vé theo chuy?n
```http
GET /api/ve/theo-chuyen/{maChuyen}
```

**Response:** List of VeDto

---

### 3.7 B?O TR? (Maintenance) - `/api/bao-tri`

#### 3.7.1 Ki?m tra tr?ng thái b?o tr?
```http
GET /api/bao-tri/trang-thai
```

**Description:** Ki?m tra xe nào c?n b?o tr?

**Algorithm:**
```sql
can_bao_tri = (DATEDIFF(NOW(), ngay_bao_tri_cuoi) > 360)
              OR (tong_km_van_hanh > nguong_km_bao_tri)
```

**Response Success (200):**
```json
{
  "success": true,
  "data": [
    {
      "maXe": "XE001",
      "tenXe": "Xe Bus 45 ch?",
      "bienSo": "30A-12345",
      "trangThai": "Ho?t ð?ng",
      "tongKmVanHanh": 52000.5,
      "ngayBaoTriCuoi": "2023-06-15",
      "soNgayTuBaoTri": 215,
      "trangThaiBaoTri": "C?n b?o tr?",
      "canBaoTri": true
    }
  ]
}
```

#### 3.7.2 Thêm l?ch b?o tr?
```http
POST /api/bao-tri
```

**Request Body:**
```json
{
  "maBaoTri": "BT001",
  "maXe": "XE001",
  "donVi": "Garage ABC",
  "chiPhi": 5000000,
  "ngay": "2024-01-15",
  "soKm": 52000
}
```

**Side Effects:**
- Reset `xe.tong_km_van_hanh = 0`
- Set `xe.ngay_bao_tri_cuoi = ngay_bao_tri`

**Response Success (200):**
```json
{
  "success": true,
  "message": "Thêm b?o tr? thành công"
}
```

#### 3.7.3 L?y l?ch s? b?o tr?
```http
GET /api/bao-tri/lich-su/{maXe}
```

**Response Success (200):**
```json
{
  "success": true,
  "data": [
    {
      "maBaoTri": "BT001",
      "maXe": "XE001",
      "donVi": "Garage ABC",
      "chiPhi": 5000000,
      "ngay": "2024-01-15",
      "soKm": 52000
    }
  ]
}
```

---

### 3.8 BÁO CÁO (Reports) - `/api/baocao`

#### 3.8.1 Chi phí cõ b?n
```http
GET /api/baocao/chi-phi-co-ban
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "maChuyen": "CX001",
      "tenChuyen": "Hà N?i - H?i Ph?ng Sáng",
      "maXe": "XE001",
      "khoangCach": 120.5,
      "chiPhiCoBan": 500000
    }
  ]
}
```

#### 3.8.2 Doanh thu xe bus ng?i theo tháng
```http
GET /api/baocao/doanh-thu-xe-bus-ngoi?tuNgay=2024-01-01&denNgay=2024-12-31
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `tuNgay` | datetime | Yes | Ngày b?t ð?u |
| `denNgay` | datetime | Yes | Ngày k?t thúc |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "maXe": "XE001",
      "thang": "2024-01",
      "doanhThuThang": 25000000
    }
  ]
}
```

#### 3.8.3 Giá vé xe bus ng?i
```http
GET /api/baocao/gia-ve-xe-bus-ngoi
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "stt": 1,
      "maChuyen": "CX001",
      "tenChuyen": "Hà N?i - H?i Ph?ng",
      "viTri": "A1",
      "maXe": "XE001",
      "chiPhiCoBan": 500000,
      "khoangCach": 120.5,
      "giaVe": 150000
    }
  ]
}
```

#### 3.8.4 Doanh thu tuy?n ðý?ng theo tháng
```http
GET /api/baocao/doanh-thu-tuyen-duong?tuNgay=2024-01-01&denNgay=2024-12-31
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "maTuyen": "TD001",
      "tenTuyen": "Hà N?i - H?i Ph?ng",
      "thang": "2024-01",
      "doanhThuThang": 75000000
    }
  ]
}
```

#### 3.8.5 Lýõng tháng tài x?
```http
GET /api/baocao/luong-thang-tai-xe?tuNgay=2024-01-01&denNgay=2024-01-31
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "maTaiXe": "TX001",
      "tenTaiXe": "Nguy?n Vãn A",
      "tongKm": 3500.5,
      "soTuyen": 25,
      "luongThang": 15000000
    }
  ]
}
```

---

### 3.9 S?N PH?M (Product) - `/api/product`

#### 3.9.1 Danh sách s?n ph?m (phân trang)
```http
GET /api/product
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `keyword` | string | T?m theo code, name |
| `categoryId` | int | L?c theo category |
| `isActive` | bool | L?c theo tr?ng thái |
| `page` | int | Trang (default: 1) |
| `size` | int | S? items (default: 20) |

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "code": "SP001",
        "name": "S?n ph?m A",
        "price": 100000,
        "categoryId": 1,
        "categoryName": "Danh m?c 1",
        "description": "Mô t? s?n ph?m",
        "stockQuantity": 100,
        "isActive": true,
        "createdDate": "2024-01-01T00:00:00",
        "updatedDate": null
      }
    ],
    "totalRecords": 50
  }
}
```

#### 3.9.2 Chi ti?t s?n ph?m
```http
GET /api/product/{id}
```

#### 3.9.3 T?o s?n ph?m
```http
POST /api/product
```

**Request Body:**
```json
{
  "code": "SP002",
  "name": "S?n ph?m B",
  "price": 200000,
  "categoryId": 1,
  "description": "Mô t?",
  "stockQuantity": 50
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": 2
}
```

#### 3.9.4 T?o nhi?u s?n ph?m (Bulk)
```http
POST /api/product/bulk
```

**Request Body:**
```json
{
  "products": [
    { "code": "SP003", "name": "SP 3", "price": 100000 },
    { "code": "SP004", "name": "SP 4", "price": 150000 }
  ]
}
```

**Response Success (200):**
```json
{
  "success": true,
  "data": {
    "totalItems": 2,
    "successCount": 2,
    "failedCount": 0,
    "allSucceeded": true
  }
}
```

#### 3.9.5 C?p nh?t s?n ph?m
```http
PUT /api/product/{id}
```

#### 3.9.6 Xóa s?n ph?m (Soft delete)
```http
DELETE /api/product/{id}
```

---

## 4. Mapping FE – BE

### 4.1 Trang Dashboard
| Component | API |
|-----------|-----|
| Th?ng kê t?ng quan | `GET /api/baocao/chi-phi-co-ban` |
| Xe c?n b?o tr? | `GET /api/bao-tri/trang-thai` |
| Chuy?n xe hôm nay | `GET /api/chuyen-xe/tim-kiem?fromDate=...&toDate=...` |

### 4.2 Qu?n l? Tài x?
| Component | API |
|-----------|-----|
| Danh sách tài x? | `GET /api/tai-xe/tim-kiem` |
| Chi ti?t tài x? | `GET /api/tai-xe/{maTaiXe}` |
| Thêm tài x? | `POST /api/tai-xe` |
| S?a tài x? | `PUT /api/tai-xe/{maTaiXe}` |
| Xóa tài x? | `DELETE /api/tai-xe/{maTaiXe}` |

### 4.3 Qu?n l? Xe
| Component | API |
|-----------|-----|
| Danh sách xe | `GET /api/xe/tim-kiem` |
| Tr?ng thái b?o tr? | `GET /api/bao-tri/trang-thai` |
| L?ch s? b?o tr? | `GET /api/bao-tri/lich-su/{maXe}` |
| Thêm b?o tr? | `POST /api/bao-tri` |

### 4.4 Qu?n l? Chuy?n xe
| Component | API |
|-----------|-----|
| Danh sách chuy?n | `GET /api/chuyen-xe/tim-kiem` |
| Vé theo chuy?n | `GET /api/ve/theo-chuyen/{maChuyen}` |
| Hoàn thành chuy?n | `PUT /api/chuyen-xe/{maChuyen}/hoan-thanh` |
| H?y chuy?n | `PUT /api/chuyen-xe/{maChuyen}/huy` |

### 4.5 Ð?t vé
| Component | API |
|-----------|-----|
| Ch?n chuy?n | `GET /api/chuyen-xe/tim-kiem` |
| Ch?n khách hàng | `GET /api/khach-hang/tim-kiem` |
| Ð?t vé | `POST /api/ve/dat-ve` |
| H?y vé | `PUT /api/ve/{stt}/huy` |

### 4.6 Báo cáo
| Báo cáo | API |
|---------|-----|
| Doanh thu xe | `GET /api/baocao/doanh-thu-xe-bus-ngoi` |
| Doanh thu tuy?n | `GET /api/baocao/doanh-thu-tuyen-duong` |
| Lýõng tài x? | `GET /api/baocao/luong-thang-tai-xe` |
| Giá vé | `GET /api/baocao/gia-ve-xe-bus-ngoi` |

---

## 5. Ghi chú cho Front-end Developer

### 5.1 X? l? Response
```javascript
// Luôn check success trý?c khi dùng data
const response = await api.get('/tai-xe/tim-kiem');
if (response.data.success) {
  setDrivers(response.data.data.items);
  setTotal(response.data.data.totalRecords);
} else {
  showError(response.data.message);
}
```

### 5.2 X? l? Paging
```javascript
const params = {
  keyword: searchText,
  pageIndex: currentPage,
  pageSize: 20,
  sortBy: 'tenTaiXe',
  sortDesc: false
};
```

### 5.3 X? l? Date
- Format g?i lên: `YYYY-MM-DD` ho?c ISO 8601
- Format nh?n v?: ISO 8601 (`2024-01-15T10:30:00`)

### 5.4 X? l? Loading/Empty/Error
```javascript
// Loading state
if (loading) return <Spinner />;

// Empty state
if (data.items.length === 0) return <EmptyState />;

// Error state
if (!response.success) return <ErrorMessage message={response.message} />;
```

### 5.5 Validation phía Client
- Validate trý?c khi g?i API ð? UX t?t hõn
- V?n ph?i handle server validation errors

---

## 6. G?i ? M? R?ng

### 6.1 API c?n thi?u (nên b? sung)
1. **Authentication/Authorization** - JWT Auth
2. **User Management** - Qu?n l? ngý?i dùng h? th?ng
3. **Audit Log** - Ghi log thao tác
4. **File Upload** - Upload h?nh ?nh xe, b?ng lái
5. **Notification** - Thông báo b?o tr?, chuy?n xe

### 6.2 C?i thi?n Response
1. Thêm `traceId` cho debugging
2. Thêm `requestId` cho tracking
3. Chu?n hóa error message ða ngôn ng?

### 6.3 Performance
1. Thêm caching cho báo cáo
2. Thêm rate limiting
3. Compression response (gzip)
