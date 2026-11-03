# GREENHOUSE LIVING - FRONTEND CLIENT

Ứng dụng Frontend cho **Hệ Thống Nhà Trọ Chuẩn Xanh (GreenHouse Living)**.
Được xây dựng bằng **React 19 + TypeScript + Vite + Tailwind CSS + Lucide Icons + Leaflet**.

Đồng bộ chuẩn xác 100% với tài liệu đặc tả Backend: `TAILIEU_API_CHO_FRONTEND.md`.

---

## I. CẤU TRÚC DỰ ÁN

```
client/
├── src/
│   ├── components/
│   │   ├── Navbar.tsx             # Header điều hướng, nút Đăng nhập / Đăng ký, Hotline, Profile
│   │   ├── HeroSection.tsx        # Banner chính, slogan, số phòng trống, trust badges
│   │   ├── RoomShowcase.tsx       # Bộ sưu tập phòng (KHÔNG HIỆN GIÁ, chỉ thông số & tiện nghi)
│   │   ├── AmenitiesSection.tsx   # 6 trụ cột an ninh & tiện ích (PCCC, vân tay, bãi xe, sạc điện...)
│   │   ├── LocationSection.tsx    # Vị trí vàng & Tiện ích ngoại khu (Leaflet Map + Tabs Chợ, BV, ĐH)
│   │   ├── BookingSection.tsx     # Form đặt lịch hẹn xem trọ thực tế
│   │   ├── FloatingActions.tsx    # Nút tròn liên hệ nổi cố định (Hotline, Zalo, Đặt lịch, Scroll top)
│   │   ├── AuthModals.tsx         # Modal Đăng nhập, Đăng ký, Tra cứu hóa đơn & VietQR
│   │   ├── Footer.tsx             # Chân trang & Thông tin pháp lý
│   │   └── Toast.tsx              # Hệ thống thông báo nổi
│   ├── services/
│   │   ├── apiClient.ts           # Axios Interceptor (Auto-Bearer, Refresh Token 401, Rate Limit 429)
│   │   ├── authService.ts         # Module 1: Auth & Identity (/api/auth)
│   │   ├── roomService.ts         # Module 4: Quản lý phòng trọ (/api/rooms)
│   │   └── invoiceService.ts      # Module 8 & 9: Hóa đơn & VietQR (/api/invoices, /api/payments)
│   ├── types/
│   │   └── index.ts               # Toàn bộ TypeScript DTO Interfaces đồng bộ từ Backend
│   ├── App.tsx                    # Main Layout & State Orchestration
│   ├── main.tsx
│   └── index.css                  # Tailwind CSS v4 & Custom Animations
├── index.html                     # Tối ưu SEO (Title, Description, OpenGraph, Schema.org JSON-LD)
└── vite.config.ts                 # Cấu hình Vite & Reverse Proxy tới .NET 9 Web API (port 7001)
```

---

## II. ĐIỂM NỔI BẬT THEO YÊU CẦU

1. **Khách vãng lai xem thoải mái không cần đăng nhập**:
   - Khách xem được toàn bộ thông số diện tích, tiện nghi, phòng trống, vị trí xung quanh.
2. **Ẩn giá tiền trước**:
   - Tập trung vào trải nghiệm khách quan, tiện ích cao cấp và nút **"Liên hệ xem phòng này"**.
3. **Cụm nút tròn liên hệ nổi (Floating Action Buttons - FAB)**:
   - Hotline gọi điện (có hiệu ứng rung lắc `animate-phone-shake` & sóng lan tỏa `animate-pulse-ring`).
   - Nút Zalo chính chủ.
   - Nút Đặt lịch hẹn xem trọ tự động cuộn đến form.
4. **Trọng tâm Vị trí & Tiện ích ngoại khu**:
   - Tabs danh mục: 🛒 Chợ & Siêu thị, 🏥 Bệnh viện & Y tế, 🎓 Trường Đại học, 🛣️ Tuyến đường chính.
   - Thể hiện chi tiết cự ly (150m, 850m, 1.3km) và thời gian di chuyển (đi bộ / xe máy).
   - Bản đồ số tương tác mượt mà bằng Leaflet & OpenStreetMap.
5. **Bảo mật & Tích hợp API chuẩn mực**:
   - Axios Interceptor tự động bắt mã `401 Unauthorized` để tự gọi API `/api/auth/refresh`.
   - Bắt mã `429 Too Many Requests` và đọc `retryAfterSeconds` để thông báo cho người dùng.
   - Tự động đính kèm `Authorization: Bearer <Token>`.

---

## III. HƯỚNG DẪN KHỞI CHẠY

```bash
# 1. Di chuyển vào thư mục client
cd client

# 2. Chạy môi trường phát triển (Dev Server)
npm run dev
```

Ứng dụng sẽ chạy tại địa chỉ: `http://localhost:3000`.  
Mọi yêu cầu `/api/*` sẽ được tự động chuyển tiếp (proxy) an toàn tới Backend .NET tại `https://localhost:7001/api`.
