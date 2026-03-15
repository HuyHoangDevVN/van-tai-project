# DANH SÁCH TÍNH NÃNG FRONT-END

## H? Th?ng Qu?n L? V?n T?i

---

## 1. Dashboard

### 1.1 Màn h?nh T?ng quan
| Thông tin | Mô t? | API |
|-----------|-------|-----|
| **M?c ðích** | Hi?n th? t?ng quan h? th?ng |
| **Th?ng kê** | T?ng xe, tài x?, chuy?n hôm nay, doanh thu |
| **C?nh báo** | Xe c?n b?o tr? | `GET /api/bao-tri/trang-thai` |
| **Chuy?n s?p t?i** | 5 chuy?n g?n nh?t | `GET /api/chuyen-xe/tim-kiem` |

**UI Behaviors:**
- Loading skeleton khi fetch data
- Auto refresh m?i 5 phút
- Click vào th? th?ng kê ? navigate t?i module týõng ?ng

---

## 2. Module Qu?n l? Tài x?

### 2.1 Danh sách Tài x?
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/drivers` |
| **API** | `GET /api/tai-xe/tim-kiem` |
| **Ch?c nãng** | Hi?n th? b?ng tài x? v?i phân trang |

**UI Components:**
- Search box (t?m theo tên, CCCD)
- Filter dropdown (gi?i tính, quê quán)
- Data table v?i sorting
- Pagination controls
- Action buttons (View, Edit, Delete)

**States:**
- Loading: Hi?n th? skeleton table
- Empty: "Không có tài x? nào"
- Error: Toast notification + Retry button

### 2.2 Chi ti?t Tài x?
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/drivers/:maTaiXe` |
| **API** | `GET /api/tai-xe/{maTaiXe}` |

**UI Components:**
- Profile card v?i thông tin cá nhân
- Tab: Thông tin chung / L?ch s? chuy?n / Th?ng kê

### 2.3 Thêm/S?a Tài x?
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/drivers/new`, `/drivers/:maTaiXe/edit` |
| **API** | `POST /api/tai-xe`, `PUT /api/tai-xe/{maTaiXe}` |

**Form Fields:**
```
- M? tài x? (required, max 20)
- Tên tài x? (max 100)
- Ngày sinh (date picker)
- Gi?i tính (dropdown: Nam/N?)
- Quê quán (text)
- Tôn giáo (text)
- S? CCCD (max 20)
- Ngày k? h?p ð?ng (date picker)
- Tu?i (number, 0-200)
- H? s? lýõng (number, 0-100)
```

**Validation:**
- Client-side: Required fields, format validation
- Server-side: Handle duplicate m? tài x?

---

## 3. Module Qu?n l? Xe

### 3.1 Danh sách Xe
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/vehicles` |
| **API** | `GET /api/xe/tim-kiem` |

**UI Components:**
- Search box (tên xe, bi?n s?)
- Filter: Tr?ng thái, H?ng s?n xu?t
- Card view ho?c Table view toggle
- Badge hi?n th? tr?ng thái (Ho?t ð?ng/B?o tr?/Ng?ng)

**Special Features:**
- Highlight xe c?n b?o tr? (màu ð?/vàng)
- Progress bar hi?n th? % km ð?n b?o tr?

### 3.2 Chi ti?t Xe
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/vehicles/:maXe` |
| **APIs** | `GET /api/xe/{maXe}`, `GET /api/bao-tri/lich-su/{maXe}` |

**Tabs:**
1. **Thông tin chung**: Specs, tr?ng thái
2. **L?ch s? b?o tr?**: Timeline b?o tr?
3. **Chuy?n xe**: Danh sách chuy?n ð? ch?y

### 3.3 Tr?ng thái B?o tr?
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/vehicles/maintenance` |
| **API** | `GET /api/bao-tri/trang-thai` |

**UI Features:**
- B?ng hi?n th? t?t c? xe
- C?t "C?n b?o tr?" v?i icon c?nh báo
- Quick action: Thêm b?o tr?

### 3.4 Thêm B?o tr?
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/vehicles/:maXe/maintenance/new` |
| **API** | `POST /api/bao-tri` |

**Form Fields:**
```
- M? b?o tr? (auto-generate ho?c manual)
- M? xe (readonly t? URL)
- Ðõn v? b?o tr? (text)
- Chi phí (currency input)
- Ngày b?o tr? (date picker, default today)
- S? km hi?n t?i (number)
```

---

## 4. Module Qu?n l? Chuy?n xe

### 4.1 Danh sách Chuy?n xe
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/trips` |
| **API** | `GET /api/chuyen-xe/tim-kiem` |

**UI Components:**
- Date range picker (filter theo ngày)
- Filter: Tuy?n ðý?ng, Xe, Tr?ng thái
- Calendar view toggle (xem theo ngày/tu?n/tháng)
- Table v?i color-coded status

**Status Colors:**
- Scheduled: Blue
- InProgress: Yellow
- Completed: Green
- Cancelled: Gray

### 4.2 Chi ti?t Chuy?n xe
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/trips/:maChuyen` |
| **APIs** | `GET /api/chuyen-xe/{maChuyen}`, `GET /api/ve/theo-chuyen/{maChuyen}` |

**UI Sections:**
1. **Header**: Thông tin chuy?n, xe, tuy?n
2. **Sõ ð? gh?**: Hi?n th? gh? ð? ð?t/tr?ng
3. **Danh sách vé**: Table vé ð? bán
4. **Actions**: Hoàn thành / H?y chuy?n

### 4.3 T?o Chuy?n xe
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/trips/new` |
| **APIs** | `GET /api/xe`, `GET /api/tuyen-duong`, `POST /api/chuyen-xe` |

**Form Fields:**
```
- M? chuy?n (auto ho?c manual)
- Tên chuy?n (text)
- Ch?n xe (dropdown with search)
- Ch?n tuy?n (dropdown with search)
- Th?i gian kh?i hành (datetime picker)
- Th?i gian ð?n d? ki?n (datetime picker)
```

### 4.4 Hoàn thành Chuy?n
| Thông tin | Mô t? |
|-----------|-------|
| **API** | `PUT /api/chuyen-xe/{maChuyen}/hoan-thanh` |

**UI Flow:**
1. Click button "Hoàn thành chuy?n"
2. Show confirmation modal
3. Loading state
4. Success: Show toast + refresh data
5. Error: Show error message

---

## 5. Module Ð?t vé

### 5.1 T?m ki?m Vé
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/tickets` |
| **API** | `GET /api/ve/tim-kiem` |

**Filters:**
- M? khách hàng
- M? chuy?n
- Tr?ng thái thanh toán
- Date range

### 5.2 Ð?t vé m?i
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/tickets/book` |
| **APIs** | Multiple |

**Booking Flow (Multi-step):**

**Step 1: Ch?n chuy?n**
- API: `GET /api/chuyen-xe/tim-kiem`
- UI: Search chuy?n theo ngày, tuy?n

**Step 2: Ch?n gh?**
- API: `GET /api/ve/theo-chuyen/{maChuyen}`
- UI: Sõ ð? gh?, click ð? ch?n

**Step 3: Thông tin khách hàng**
- API: `GET /api/khach-hang/tim-kiem` (search existing)
- UI: Form nh?p thông tin ho?c ch?n khách c?

**Step 4: Xác nh?n & Thanh toán**
- API: `POST /api/ve/dat-ve`
- UI: Summary, ch?n phýõng th?c thanh toán

**Step 5: Hoàn t?t**
- UI: Success message, in vé

### 5.3 H?y vé
| Thông tin | Mô t? |
|-----------|-------|
| **API** | `PUT /api/ve/{stt}/huy` |

**UI Flow:**
- Confirmation modal v?i l? do h?y
- Check ði?u ki?n h?y (chuy?n chýa hoàn thành)

---

## 6. Module Khách hàng

### 6.1 Danh sách Khách hàng
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/customers` |
| **API** | `GET /api/khach-hang/tim-kiem` |

**UI Features:**
- Search by name, phone, CCCD
- Sort by total spending (VIP customers)
- Quick view booking history

### 6.2 Chi ti?t Khách hàng
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/customers/:maKhach` |

**Tabs:**
1. Thông tin cá nhân
2. L?ch s? ð?t vé
3. Th?ng kê chi tiêu

---

## 7. Module Tuy?n ðý?ng

### 7.1 Danh sách Tuy?n
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/routes` |
| **API** | `GET /api/tuyen-duong/tim-kiem` |

**UI Features:**
- Filter by ði?m ði, ði?m ð?n
- Map view (optional)
- Stats: S? chuy?n, doanh thu

### 7.2 Chi ti?t Tuy?n
- Thông tin tuy?n
- Danh sách chuy?n xe
- Th?ng kê doanh thu

---

## 8. Module Báo cáo

### 8.1 Báo cáo Doanh thu
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/reports/revenue` |

**Sub-reports:**
1. **Doanh thu theo xe**: `GET /api/baocao/doanh-thu-xe-bus-ngoi`
2. **Doanh thu theo tuy?n**: `GET /api/baocao/doanh-thu-tuyen-duong`

**UI Features:**
- Date range picker
- Bar chart / Line chart
- Export PDF/Excel

### 8.2 Báo cáo Lýõng
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/reports/salary` |
| **API** | `GET /api/baocao/luong-thang-tai-xe` |

**UI Features:**
- Month picker
- Table v?i t?ng lýõng
- Sort by lýõng, s? chuy?n

### 8.3 Báo cáo Giá vé
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/reports/pricing` |
| **API** | `GET /api/baocao/gia-ve-xe-bus-ngoi` |

### 8.4 Báo cáo Chi phí
| Thông tin | Mô t? |
|-----------|-------|
| **Route** | `/reports/costs` |
| **API** | `GET /api/baocao/chi-phi-co-ban` |

---

## 9. Settings (Future)

### 9.1 Qu?n l? User
- Danh sách user
- Phân quy?n

### 9.2 C?u h?nh h? th?ng
- Ngý?ng km b?o tr?
- C?u h?nh giá vé
- Ð? ph?c t?p tuy?n ðý?ng

---

## 10. UI/UX Guidelines

### 10.1 Loading States
```jsx
// Table loading
<TableSkeleton rows={10} columns={5} />

// Card loading
<CardSkeleton />

// Button loading
<Button loading>Ðang x? l?...</Button>
```

### 10.2 Empty States
```jsx
<EmptyState
  icon={<TruckIcon />}
  title="Không có xe nào"
  description="B?t ð?u b?ng cách thêm xe m?i"
  action={<Button>Thêm xe</Button>}
/>
```

### 10.3 Error States
```jsx
<ErrorState
  title="Không th? t?i d? li?u"
  description={error.message}
  action={<Button onClick={retry}>Th? l?i</Button>}
/>
```

### 10.4 Toast Notifications
```jsx
// Success
toast.success('Thêm tài x? thành công');

// Error
toast.error('Không th? xóa. Tài x? ðang có chuy?n.');

// Warning
toast.warning('Xe này c?n b?o tr?');
```

### 10.5 Confirmation Modals
```jsx
<ConfirmModal
  title="Xác nh?n xóa"
  description="B?n có ch?c mu?n xóa tài x? này?"
  confirmText="Xóa"
  cancelText="H?y"
  onConfirm={handleDelete}
  danger
/>
```

---

## 11. Responsive Breakpoints

| Breakpoint | Width | Layout |
|------------|-------|--------|
| Mobile | < 640px | Single column, bottom nav |
| Tablet | 640-1024px | Collapsible sidebar |
| Desktop | > 1024px | Full sidebar, multi-column |

---

## 12. Accessibility Requirements

- All interactive elements focusable
- Keyboard navigation support
- ARIA labels for icons
- Color contrast AA compliant
- Screen reader friendly tables
