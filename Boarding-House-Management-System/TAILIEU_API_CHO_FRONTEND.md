# BÁO CÁO ĐẶC TẢ API & MÃ LỖI HỆ THỐNG QUẢN LÝ NHÀ TRỌ (CHO FRONTEND)

> **Dự án**: Boarding House Management System  
> **Kiến trúc**: Clean Architecture (.NET 9 Web API)  
> **Phiên bản API**: v1.0  
> **Ngày cập nhật**: 03/11/2026  
> **Tài liệu dành cho**: Đội ngũ Frontend (React / Vue / Mobile)

---

## I. QUY CHUẨN KỸ THUẬT CHUNG

### 1. Base URL & Headers
- **Base URL**: `https://localhost:7001/api` (hoặc cấu hình môi trường Development / Production)
- **Content-Type**: `application/json`
- **Authentication**: Chuẩn **JWT Bearer Token**
  ```http
  Authorization: Bearer <AccessToken>
  ```

### 2. Cấu trúc Phân trang (PagedResult)
Tất cả các API trả về danh sách có phân trang đều tuân thủ cấu trúc đồng nhất sau:
```json
{
  "items": [ /* Danh sách đối tượng */ ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 3. Cấu trúc Lỗi chuẩn (Error Response Schema)
Khi có lỗi xảy ra (HTTP StatusCode >= 400), API trả về JSON chuẩn theo format:
```json
{
  "statusCode": 400,
  "title": "Tên loại lỗi",
  "message": "Nội dung giải thích lỗi chi tiết hiển thị cho người dùng",
  "errors": null,
  "path": "/api/rooms/10",
  "timestamp": "2026-11-03T14:30:00Z"
}
```

---

## II. BẢNG MÃ LỖI HTTP & Ý NGHĨA

| HTTP Code | Tiêu đề | Ý nghĩa & Hành vi Frontend đề xuất |
| :--- | :--- | :--- |
| **`200 OK`** | Thành công | Yêu cầu được thực hiện thành công, trả về dữ liệu. |
| **`201 Created`** | Đã tạo thành công | Tạo mới tài nguyên thành công (kèm ID vừa tạo). |
| **`204 NoContent`** | Thành công không có nội dung | Xóa hoặc cập nhật thành công, không cần reload dữ liệu thừa. |
| **`400 Bad Request`** | Dữ liệu không hợp lệ | Thiếu trường bắt buộc, số điện/nước mới < số cũ, sai logic. Hiển thị Toast thông báo `message`. |
| **`401 Unauthorized`** | Chưa xác thực | Token hết hạn hoặc không có token. **FE tự động gọi API Refresh Token hoặc chuyển hướng sang màn Login**. |
| **`403 Forbidden`** | Không có quyền truy cập | Sai quyền vai trò (vd: Khách thuê cố tình gọi API xóa phòng của Chủ trọ). Hiển thị trang 403. |
| **`404 Not Found`** | Không tìm thấy tài nguyên | Không tìm thấy phòng, hóa đơn, dãy trọ với ID tương ứng. Hiển thị thông báo hoặc Empty State. |
| **`409 Conflict`** | Xung đột dữ liệu | Trùng số phòng trong cùng dãy trọ, trùng CCCD, phòng đã có hợp đồng thuê. Hiển thị cảnh báo trực tiếp trên Form. |
| **`429 Too Many Requests`** | Quá nhiều yêu cầu (Rate Limit) | Người dùng gửi request quá nhanh. FE khóa nút bấm và đếm ngược theo `retryAfterSeconds`. |
| **`500 Internal Server Error`** | Lỗi máy chủ | Lỗi crash hệ thống phía Backend. Hiển thị thông báo "Hệ thống đang bảo trì, vui lòng thử lại sau". |

---

## III. CHÍNH SÁCH RATE LIMITING (GIỚI HẠN TẦN SUẤT GỌI)

Backend đã cấu hình 4 Policy kiểm soát tần suất tự động:
1. **`AuthPolicy`** (10 requests / 1 phút): Áp dụng cho Login, Register, Refresh Token, Đổi mật khẩu. Chống tấn công dò mật khẩu (Brute-Force).
2. **`StandardPolicy`** (60 requests / 1 phút): Áp dụng cho các màn hình CRUD danh sách phòng, khách trọ, hóa đơn.
3. **`HeavyPolicy`** (15 requests / 1 phút): Áp dụng cho thao tác sinh hóa đơn cả dãy trọ, chốt số hàng loạt 20 phòng, tải biểu đồ doanh thu.
4. **`WebhookPolicy`** (120 requests / 1 phút): Dành cho cổng tự động nhận biến động số dư SePay / Ngân hàng.

**Format lỗi 429:**
```json
{
  "statusCode": 429,
  "title": "Quá nhiều yêu cầu (Rate Limit Exceeded)",
  "message": "Bạn đã gửi quá nhiều yêu cầu trong thời gian ngắn. Vui lòng thử lại sau.",
  "retryAfterSeconds": 45
}
```

---

## IV. CHI TIẾT TẤT CẢ CÁC API THEO MODULE

---

### MODULE 1: AUTHENTICATION & IDENTITY (`/api/auth`)
*Phục vụ: Màn hình Đăng ký, Đăng nhập, Duy trì phiên làm việc.*

#### 1. Đăng ký tài khoản
- **Method / Route**: `POST /api/auth/register`
- **Quyền**: Public
- **Rate Limit**: `AuthPolicy`
- **Request Body**:
  ```json
  {
    "username": "0987654321",
    "password": "Password123@",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0987654321",
    "citizenId": "079201001234",
    "role": "Tenant" // "Tenant" hoặc "Landlord"
  }
  ```
- **Response `200 OK`**:
  ```json
  {
    "token": "eyJhbGciOi...",
    "refreshToken": "d8e3b...",
    "expiresAt": "2026-11-03T15:30:00Z",
    "user": {
      "userId": "guid...",
      "fullName": "Nguyễn Văn A",
      "phoneNumber": "0987654321",
      "role": "Tenant"
    }
  }
  ```
- **Lỗi thường gặp**:
  - `400 Bad Request`: Số điện thoại sai định dạng, mật khẩu ngắn hơn 6 ký tự.
  - `409 Conflict`: Số điện thoại hoặc CCCD đã được đăng ký.

#### 2. Đăng nhập
- **Method / Route**: `POST /api/auth/login`
- **Quyền**: Public
- **Rate Limit**: `AuthPolicy`
- **Request Body**:
  ```json
  {
    "username": "0987654321",
    "password": "Password123@"
  }
  ```
- **Response `200 OK`**: Giống Response Đăng ký.
- **Lỗi thường gặp**:
  - `400 Bad Request`: Sai số điện thoại hoặc mật khẩu.
  - `403 Forbidden`: Tài khoản đã bị khóa vĩnh viễn bởi Admin.

#### 3. Làm mới Token (Refresh Token)
- **Method / Route**: `POST /api/auth/refresh`
- **Quyền**: Public
- **Rate Limit**: `AuthPolicy`
- **Request Body**:
  ```json
  {
    "token": "accessToken_cũ_hết_hạn",
    "refreshToken": "refreshToken_đang_lưu"
  }
  ```
- **Response `200 OK`**: Cặp token mới `{ token, refreshToken, expiresAt }`.
- **Lỗi thường gặp**:
  - `401 Unauthorized`: Refresh Token không hợp lệ hoặc đã bị thu hồi do đổi mật khẩu / bị Admin khóa. FE cần redirect về Login.

#### 4. Đăng xuất
- **Method / Route**: `POST /api/auth/logout`
- **Quyền**: Public
- **Request Body**: `{ "refreshToken": "..." }`
- **Response `200 OK`**: `{ "message": "Đăng xuất thành công." }`

#### 5. Lấy thông tin tài khoản đang đăng nhập
- **Method / Route**: `GET /api/auth/me`
- **Quyền**: `[Authorize]`
- **Response `200 OK`**: Thông tin cá nhân và quyền hạn của người dùng.

---

### MODULE 2: QUẢN LÝ TÀI KHOẢN & NGƯỜI DÙNG (`/api/users`)
*Phục vụ: Màn hình Cài đặt cá nhân, Quản lý tài khoản Admin.*

#### 1. Xem Profile của mình
- **Method / Route**: `GET /api/users/profile`
- **Quyền**: `[Authorize]` (Tenant, Landlord, Admin)
- **Response `200 OK`**:
  ```json
  {
    "userId": "guid...",
    "userName": "0987654321",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0987654321",
    "citizenId": "079201001234",
    "email": "vana@gmail.com",
    "dateOfBirth": "2000-05-15T00:00:00Z",
    "gender": "Nam",
    "address": "TP. Hồ Chí Minh",
    "avatarUrl": "https://...",
    "role": "Tenant",
    "isActive": true,
    "currentResidence": {
      "roomId": 10,
      "roomNumber": "101",
      "houseId": 1,
      "houseName": "Nhà Trọ An Bình",
      "houseAddress": "123 Đường Số 1, Q. Bình Thạnh",
      "checkInDate": "2026-01-01T00:00:00Z",
      "contractEndDate": "2027-01-01T00:00:00Z",
      "rentalPrice": 3500000,
      "depositAmount": 3500000,
      "contractNumber": "HD-20260101-P101-1234"
    }
  }
  ```

#### 2. Cập nhật Profile cá nhân
- **Method / Route**: `PUT /api/users/profile`
- **Quyền**: `[Authorize]`
- **Request Body**:
  ```json
  {
    "fullName": "Nguyễn Văn A",
    "citizenId": "079201001234",
    "phoneNumber": "0987654321",
    "email": "vana@gmail.com",
    "dateOfBirth": "2000-05-15T00:00:00Z",
    "gender": "Nam",
    "address": "TP. Hồ Chí Minh",
    "avatarUrl": "https://..."
  }
  ```
- **Response `200 OK`**: `{ "message": "Cập nhật thông tin cá nhân thành công." }`

#### 3. Đổi mật khẩu
- **Method / Route**: `PUT /api/users/change-password`
- **Quyền**: `[Authorize]`
- **Rate Limit**: `AuthPolicy`
- **Request Body**:
  ```json
  {
    "currentPassword": "OldPassword123@",
    "newPassword": "NewPassword123@",
    "confirmNewPassword": "NewPassword123@"
  }
  ```
- **Response `200 OK`**: `{ "message": "Đổi mật khẩu thành công..." }`

#### 4. [Admin] Xem danh sách người dùng (Kèm thông tin phòng, trọ, ngày vào)
- **Method / Route**: `GET /api/users`
- **Quyền**: `[Authorize(Roles = "Admin")]`
- **Query Params**:
  - `keyword`: Tìm theo tên, SĐT, CCCD, Email
  - `role`: "Tenant", "Landlord", "Admin"
  - `isActive`: true/false
  - `houseId`: ID dãy trọ để lọc khách ở dãy đó
  - `pageNumber`: 1, `pageSize`: 10
- **Response `200 OK`**: Cấu trúc `PagedResult<AdminUserListItemResponse>` hiển thị người dùng và mục `currentResidence`.

#### 5. [Admin] Chi tiết người dùng & Lịch sử thuê phòng
- **Method / Route**: `GET /api/users/{id}`
- **Quyền**: `[Authorize(Roles = "Admin")]`
- **Response `200 OK`**: Đầy đủ thông tin profile + mảng `rentalHistory` (danh sách toàn bộ các phòng đã và đang thuê).

#### 6. [Admin] Khóa tài khoản vĩnh viễn
- **Method / Route**: `POST /api/users/{id}/lock`
- **Quyền**: `[Authorize(Roles = "Admin")]`
- **Request Body**: `{ "reason": "Vi phạm nội quy nhiều lần" }`
- **Response `200 OK`**: `{ "message": "Đã khóa tài khoản vĩnh viễn và thu hồi toàn bộ phiên đăng nhập..." }`

#### 7. [Admin] Mở khóa tài khoản
- **Method / Route**: `POST /api/users/{id}/unlock`
- **Quyền**: `[Authorize(Roles = "Admin")]`
- **Response `200 OK`**: `{ "message": "Mở khóa tài khoản thành công." }`

#### 8. [Admin] Cấp lại mật khẩu cho tài khoản
- **Method / Route**: `POST /api/users/{id}/reset-password`
- **Quyền**: `[Authorize(Roles = "Admin")]`
- **Request Body**:
  ```json
  {
    "newPassword": "ResetPass2026@",
    "confirmNewPassword": "ResetPass2026@"
  }
  ```
- **Response `200 OK`**: `{ "message": "Cấp lại mật khẩu thành công..." }`

---

### MODULE 3: QUẢN LÝ DÃY TRỌ (`/api/houses`)
*Phục vụ: Thiết lập chi nhánh / dãy trọ, cài đặt biểu giá mặc định.*

- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Rate Limit**: `StandardPolicy`

| Endpoint | Method | Mô tả & Tham số | Response |
| :--- | :--- | :--- | :--- |
| `/api/houses` | `GET` | Danh sách các dãy trọ của chủ trọ | `200 OK`: Mảng các dãy trọ kèm `totalRooms`, `occupiedRooms`, `availableRooms`. |
| `/api/houses/{id}` | `GET` | Chi tiết 1 dãy trọ và biểu phí mặc định | `200 OK`: Chi tiết đơn giá điện, nước, wifi, rác, gửi xe. |
| `/api/houses` | `POST` | Tạo dãy trọ mới | `201 Created`: `{ "houseId": 1, "message": "..." }` |
| `/api/houses/{id}` | `PUT` | Cập nhật thông tin và biểu giá | `200 OK`: `{ "message": "Cập nhật..." }` |
| `/api/houses/{id}` | `DELETE` | Xóa dãy trọ *(Chặn nếu dãy còn phòng)* | `204 NoContent`. *(Lỗi 400 nếu dãy đang có phòng)* |

**Body mẫu khi Tạo/Sửa Dãy trọ (`POST`/`PUT`):**
```json
{
  "name": "Nhà Trọ An Bình - Chi nhánh 1",
  "address": "123 Đường Số 1, P. 25, Q. Bình Thạnh, TP.HCM",
  "defaultElectricityPrice": 3500,
  "defaultWaterPrice": 20000,
  "defaultInternetPrice": 80000,
  "defaultCleaningPrice": 30000,
  "defaultParkingPrice": 120000,
  "description": "Nhà trọ an ninh, camera 24/7, giờ giấc tự do"
}
```

---

### MODULE 4: QUẢN LÝ PHÒNG TRỌ (`/api/rooms`)
*Phục vụ: Sơ đồ phòng, lọc phòng trống, thêm sửa phòng, cập nhật trạng thái.*

#### 1. Lọc và phân trang phòng trọ
- **Method / Route**: `GET /api/rooms`
- **Quyền**: `[Authorize]` (Cả Landlord lẫn Tenant)
- **Query Params**:
  - `houseId`: ID dãy trọ (int)
  - `status`: "Available", "Occupied", "Reserved", "UnderMaintenance"
  - `floor`: Tầng (int, 0 là tầng trệt)
  - `minPrice`: Giá thấp nhất, `maxPrice`: Giá cao nhất
  - `pageNumber`: 1, `pageSize`: 10
- **Response `200 OK`**: Cấu trúc `PagedResult<RoomResponse>`.

#### 2. Chi tiết phòng (Kèm hợp đồng đang thuê)
- **Method / Route**: `GET /api/rooms/{id}`
- **Quyền**: `[Authorize]`
- **Response `200 OK`**:
  ```json
  {
    "roomId": 5,
    "houseId": 1,
    "houseName": "Nhà Trọ An Bình",
    "roomNumber": "101",
    "price": 3500000,
    "floor": 1,
    "size": 25.5,
    "maxRenters": 3,
    "hasBalcony": true,
    "status": 1, // 0: Available, 1: Occupied, 2: Reserved, 3: UnderMaintenance
    "statusName": "Occupied",
    "activeContract": {
      "contractId": 12,
      "contractNumber": "HD-20260101-P101-1234",
      "tenantId": "guid...",
      "tenantName": "Nguyễn Văn A",
      "tenantPhone": "0987654321",
      "startDate": "2026-01-01T00:00:00Z",
      "endDate": "2027-01-01T00:00:00Z",
      "rentalPrice": 3500000,
      "depositAmount": 3500000
    }
  }
  ```

#### 3. Thêm mới phòng trọ
- **Method / Route**: `POST /api/rooms`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Request Body**:
  ```json
  {
    "houseId": 1,
    "roomNumber": "102",
    "price": 3500000,
    "floor": 1,
    "size": 22.0,
    "maxRenters": 2,
    "hasBalcony": false
  }
  ```
- **Response `201 Created`**: `{ "roomId": 6, "message": "Thêm phòng trọ thành công." }`
- **Lỗi `409 Conflict`**: `"Phòng số '102' đã tồn tại trong dãy trọ này."`

#### 4. Cập nhật phòng trọ
- **Method / Route**: `PUT /api/rooms/{id}`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Response `204 NoContent`**

#### 5. Đổi trạng thái phòng (Bảo trì / Trống)
- **Method / Route**: `PATCH /api/rooms/{id}/status`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Request Body**: `{ "status": 3 }` *(3: UnderMaintenance, 0: Available)*
- **Response `200 OK`**: `{ "message": "Cập nhật trạng thái phòng thành công." }`

#### 6. Xóa phòng
- **Method / Route**: `DELETE /api/rooms/{id}`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Response `204 NoContent`**
- **Lỗi `400 Bad Request`**: `"Không thể xóa phòng trọ đang có người thuê. Vui lòng thanh lý hợp đồng trước."`

---

### MODULE 5: HỒ SƠ KHÁCH THUÊ (`/api/tenants`)
*Phục vụ: Tìm kiếm nhanh khách trọ theo CCCD/SĐT, tạo hồ sơ khách trọ mới.*

- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Rate Limit**: `StandardPolicy`

| Endpoint | Method | Tham số / Body | Response |
| :--- | :--- | :--- | :--- |
| `/api/tenants` | `GET` | `?keyword=0987&roomId=5` | `200 OK`: Danh sách khách thuê khớp từ khóa. |
| `/api/tenants` | `POST` | Body: `{ fullName, phoneNumber, citizenId, email, dateOfBirth, hometown, gender }` | `201 Created`: `{ "tenantId": "...", "message": "..." }` |
| `/api/tenants/{id}`| `PUT` | Body: `{ fullName, phoneNumber, citizenId, email, dateOfBirth, address, gender }` | `200 OK`: Cập nhật thành công. |

---

### MODULE 6: HỢP ĐỒNG THUÊ PHÒNG (`/api/contracts`)
*Phục vụ: Ký hợp đồng nhận phòng, gia hạn, cảnh báo hết hạn, thanh lý trả cọc.*

- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`

#### 1. Lập hợp đồng nhận phòng
- **Method / Route**: `POST /api/contracts`
- **Nghiệp vụ**: Hệ thống tự động kích hoạt hợp đồng $\rightarrow$ Thu cọc $\rightarrow$ Đổi trạng thái phòng sang `Occupied`.
- **Request Body**:
  ```json
  {
    "roomId": 5,
    "tenantId": "user_guid_123",
    "startDate": "2026-11-01T00:00:00Z",
    "endDate": "2027-11-01T00:00:00Z",
    "rentalPrice": 3500000,
    "depositAmount": 3500000,
    "numberOfRenters": 2,
    "numberOfVehicles": 2,
    "initialElectricityIndex": 1250,
    "initialWaterIndex": 84,
    "electricityUnitPrice": 3500,
    "waterUnitPrice": 20000,
    "internetUnitPrice": 80000,
    "cleaningUnitPrice": 30000,
    "parkingUnitPrice": 120000,
    "note": "Khách dọn vào ngày 01/11"
  }
  ```
- **Response `201 Created`**: `{ "contractId": 15, "message": "Lập hợp đồng và kích hoạt nhận phòng thành công." }`
- **Lỗi `409 Conflict`**: `"Phòng '101' hiện đang có hợp đồng thuê còn hiệu lực."`

#### 2. Danh sách hợp đồng đang hiệu lực
- **Method / Route**: `GET /api/contracts/active`
- **Response `200 OK`**: Mảng danh sách các hợp đồng `ContractStatus = Active`.

#### 3. Danh sách hợp đồng sắp hết hạn
- **Method / Route**: `GET /api/contracts/expiring?days=30`
- **Response `200 OK`**: Danh sách hợp đồng hết hạn trong vòng `days` ngày (kèm số ngày còn lại `daysRemaining`).

#### 4. Thanh lý hợp đồng / Trả phòng
- **Method / Route**: `POST /api/contracts/{id}/terminate`
- **Nghiệp vụ**: Chốt ngày trả phòng $\rightarrow$ Trừ chi phí hư hỏng $\rightarrow$ Hoàn cọc $\rightarrow$ Đổi trạng thái phòng thành `Available`.
- **Request Body**:
  ```json
  {
    "returnDepositAmount": 3200000,
    "deductions": 300000,
    "reason": "Khấu trừ làm trầy xước tường và hỏng bóng đèn",
    "actualEndDate": "2026-11-03T00:00:00Z"
  }
  ```
- **Response `200 OK`**: `{ "message": "Thanh lý hợp đồng và trả phòng thành công." }`

---

### MODULE 7: CHỈ SỐ ĐIỆN NƯỚC (`/api/meter-readings`)
*Phục vụ: Màn hình ghi số điện nước hàng tháng, cảnh báo bất thường.*

- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`

#### 1. Lấy bảng chỉ số theo dãy trọ và tháng
- **Method / Route**: `GET /api/meter-readings?houseId=1&month=11&year=2026`
- **Response `200 OK`**: Mảng chỉ số từng phòng kèm số kWh, m³ đã sử dụng và cờ `isBilled`.

#### 2. Lấy chỉ số tháng trước (Tự động điền "Số cũ")
- **Method / Route**: `GET /api/meter-readings/rooms/{roomId}/previous?month=11&year=2026`
- **Response `200 OK`**:
  ```json
  {
    "roomId": 5,
    "previousMonth": 10,
    "previousYear": 2026,
    "lastElectricityIndex": 1250,
    "lastWaterIndex": 84
  }
  ```

#### 3. Ghi chỉ số 1 phòng (Có thuật toán phát hiện bất thường)
- **Method / Route**: `POST /api/meter-readings`
- **Request Body**:
  ```json
  {
    "roomId": 5,
    "month": 11,
    "year": 2026,
    "oldElectricity": 1250,
    "newElectricity": 1420,
    "oldWater": 84,
    "newWater": 92,
    "note": "Ghi số định kỳ"
  }
  ```
- **Response `201 Created`**:
  ```json
  {
    "meterReadingId": 34,
    "electricityUsage": 170,
    "waterUsage": 8,
    "isAnomaly": false,
    "anomalyWarning": null
  }
  ```
  *(Nếu chỉ số điện tăng đột biến $\ge 3$ lần, `isAnomaly: true` và `anomalyWarning` có thông điệp cảnh báo để FE highlight màu cam/đỏ trên bảng).*

#### 4. Ghi chỉ số hàng loạt (Bulk Record)
- **Method / Route**: `POST /api/meter-readings/bulk`
- **Rate Limit**: `HeavyPolicy`
- **Request Body**:
  ```json
  {
    "houseId": 1,
    "month": 11,
    "year": 2026,
    "readings": [
      { "roomId": 1, "oldElectricity": 100, "newElectricity": 150, "oldWater": 20, "newWater": 25 },
      { "roomId": 2, "oldElectricity": 210, "newElectricity": 300, "oldWater": 45, "newWater": 55 }
    ]
  }
  ```
- **Response `200 OK`**: `{ "successCount": 2, "failureCount": 0, "errors": [] }`

---

### MODULE 8: HÓA ĐƠN & TÍNH TIỀN (`/api/invoices`)
*Phục vụ: Tự động tính tiền phòng, xuất phiếu thu, hủy hóa đơn sai sót.*

#### 1. Sinh hóa đơn tự động hàng loạt cả dãy trọ
- **Method / Route**: `POST /api/invoices/generate-monthly`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Rate Limit**: `HeavyPolicy`
- **Nghiệp vụ**: Quét toàn bộ phòng đang thuê $\rightarrow$ Áp dụng biểu giá / Strategy (Standard hoặc Tiered điện bậc thang) $\rightarrow$ Khóa chỉ số điện nước không cho sửa.
- **Request Body**:
  ```json
  {
    "houseId": 1,
    "month": 11,
    "year": 2026,
    "dueDate": "2026-11-05T23:59:59Z",
    "strategyType": "Standard" // "Standard" hoặc "Tiered"
  }
  ```
- **Response `201 Created`**:
  ```json
  {
    "houseId": 1,
    "month": 11,
    "year": 2026,
    "totalInvoicesCreated": 12,
    "totalAmountBilled": 54200000,
    "skippedRoomsInfo": []
  }
  ```

#### 2. Lọc danh sách hóa đơn
- **Method / Route**: `GET /api/invoices`
- **Quyền**: `[Authorize]`
  - *Nếu là Tenant*: Hệ thống tự động chặn và chỉ trả về hóa đơn đứng tên khách đó.
  - *Nếu là Landlord/Admin*: Xem toàn bộ.
- **Query Params**: `?houseId=1&month=11&year=2026&status=Unpaid&roomId=5`
  *(Status: 0: Unpaid, 1: PartiallyPaid, 2: Paid, 3: Overdue, 4: Cancelled)*
- **Response `200 OK`**: Mảng danh sách hóa đơn tóm tắt.

#### 3. Chi tiết 1 hóa đơn đầy đủ các khoản thu
- **Method / Route**: `GET /api/invoices/{id}`
- **Quyền**: `[Authorize]`
- **Response `200 OK`**:
  ```json
  {
    "invoiceId": 18,
    "invoiceCode": "HDT-202611-P101",
    "contractNumber": "HD-20260101-P101-1234",
    "roomNumber": "101",
    "houseName": "Nhà Trọ An Bình",
    "houseAddress": "123 Đường Số 1, Q. Bình Thạnh",
    "tenantName": "Nguyễn Văn A",
    "month": 11,
    "year": 2026,
    "issueDate": "2026-11-01T00:00:00Z",
    "dueDate": "2026-11-05T00:00:00Z",
    "roomPrice": 3500000,
    "electricityUsage": 120,
    "electricityUnitPrice": 3500,
    "electricityCost": 420000,
    "waterUsage": 6,
    "waterUnitPrice": 20000,
    "waterCost": 120000,
    "internetCost": 80000,
    "cleaningCost": 30000,
    "parkingCost": 120000,
    "otherFees": 0,
    "discountAmount": 0,
    "totalAmount": 4270000,
    "paidAmount": 0,
    "remainingAmount": 4270000,
    "status": 0,
    "statusName": "Unpaid",
    "payments": []
  }
  ```

#### 4. Hủy hóa đơn (Sửa sai chỉ số)
- **Method / Route**: `PATCH /api/invoices/{id}/cancel`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Nghiệp vụ**: Hủy hóa đơn và tự động mở khóa chỉ số điện nước để chủ trọ chốt lại.
- **Request Body**: `"Ghi nhầm chỉ số nước phòng 101"`
- **Response `200 OK`**: `{ "message": "Hủy hóa đơn thành công và đã mở khóa chỉ số..." }`

---

### MODULE 9: THANH TOÁN ONLINE & TIỀN MẶT (`/api/payments`)
*Phục vụ: Quét mã QR chuyển khoản ngân hàng, thu tiền mặt, tự động gạch nợ SePay.*

#### 1. Lấy mã VietQR động theo hóa đơn
- **Method / Route**: `GET /api/payments/{invoiceId}/vietqr`
- **Quyền**: `[Authorize]`
- **Response `200 OK`**:
  ```json
  {
    "invoiceId": 18,
    "invoiceCode": "HDT-202611-P101",
    "amount": 4270000,
    "bankAccountNo": "0987654321",
    "bankCode": "MB",
    "bankName": "MBBank",
    "accountName": "CHU NHA TRO",
    "description": "PHONG101 T11 HDT-202611-P101",
    "qrImageUrl": "https://img.vietqr.io/image/MB-0987654321-compact2.png?amount=4270000&addInfo=PHONG101+T11+HDT-202611-P101&accountName=CHU+NHA+TRO",
    "qrQuickLink": "https://api.vietqr.io/MB/0987654321/4270000/..."
  }
  ```
  *(FE chỉ cần đưa `qrImageUrl` vào thẻ `<img src={data.qrImageUrl} />` là hiển thị mã QR đẹp mắt có sẵn logo ngân hàng và thông tin chuyển khoản).*

#### 2. Xác nhận thu tiền mặt trực tiếp
- **Method / Route**: `POST /api/payments/cash`
- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Request Body**:
  ```json
  {
    "invoiceId": 18,
    "amountPaid": 4270000,
    "note": "Khách đưa tiền mặt tại phòng"
  }
  ```
- **Response `200 OK`**:
  ```json
  {
    "paymentId": 25,
    "paymentCode": "PAY-CASH-20261103-18",
    "amount": 4270000,
    "method": 0,
    "methodName": "Cash",
    "status": 1,
    "statusName": "Success",
    "remainingInvoiceAmount": 0
  }
  ```

#### 3. Webhook SePay nhận thanh toán tự động
- **Method / Route**: `POST /api/payments/sepay-webhook`
- **Quyền**: `[AllowAnonymous]`
- **Rate Limit**: `WebhookPolicy`
- **Nghiệp vụ**: Nhận tín hiệu từ SePay khi có biến động số dư ngân hàng $\rightarrow$ So khớp `content` với mã `HDT-...` $\rightarrow$ Tự động gạch nợ hóa đơn sang `Paid`.

---

### MODULE 10: BÁO CÁO THỐNG KÊ DASHBOARD (`/api/dashboard`)
*Phục vụ: Màn hình Tổng quan Dashboard, biểu đồ doanh thu Chart.js.*

- **Quyền**: `[Authorize(Roles = "Landlord,Admin")]`
- **Rate Limit**: `HeavyPolicy`

#### 1. Thống kê tổng quan tháng
- **Method / Route**: `GET /api/dashboard/summary?houseId=1&month=11&year=2026`
- **Response `200 OK`**:
  ```json
  {
    "houseId": 1,
    "month": 11,
    "year": 2026,
    "expectedRevenue": 54200000,        // Doanh thu dự kiến
    "actualRevenueCollected": 42100000, // Đã thu thực tế
    "totalUnpaidDebt": 12100000,        // Tiền nợ chưa thu
    "roomStatus": {
      "totalRooms": 20,
      "occupiedRooms": 15,
      "availableRooms": 3,
      "maintenanceRooms": 2,
      "reservedRooms": 0
    }
  }
  ```

#### 2. Biểu đồ doanh thu 12 tháng (Dùng cho Chart.js)
- **Method / Route**: `GET /api/dashboard/revenue-chart?houseId=1&year=2026`
- **Response `200 OK`**:
  ```json
  [
    { "month": 1, "totalRevenue": 48000000 },
    { "month": 2, "totalRevenue": 49500000 },
    { "month": 3, "totalRevenue": 52000000 },
    { "month": 11, "totalRevenue": 42100000 }
  ]
  ```
  *(FE chỉ cần đưa mảng này vào trục `labels: [1..12]` và `data: [totalRevenue]` của Chart.js / Recharts).*

---

## V. CHECKLIST TÍCH HỢP CHO FRONTEND

1. **Lưu trữ Token**: Lưu `token` và `refreshToken` trong `localStorage` hoặc `Cookie Secure`.
2. **Axios Interceptor**:
   - Tự động gắn header `Authorization: Bearer <token>` vào mọi request.
   - Bắt lỗi `401 Unauthorized`: Tự động gọi `POST /api/auth/refresh` bằng refreshToken. Nếu thành công thì retry request cũ; nếu thất bại thì xóa storage và chuyển về `/login`.
   - Bắt lỗi `429 Too Many Requests`: Đọc giá trị `retryAfterSeconds` trong response và hiển thị đồng hồ đếm ngược trên giao diện.
3. **Phân quyền Route (Protected Routes)**:
   - Route `/admin/*`: Kiểm tra `user.role === 'Admin'`.
   - Route `/landlord/*`: Kiểm tra `user.role === 'Landlord' || user.role === 'Admin'`.
   - Route `/tenant/*`: Kiểm tra `user.role === 'Tenant'`.
