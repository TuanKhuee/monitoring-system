#  Web Service Monitoring System - Backend API

Hệ thống giám sát trạng thái hoạt động (uptime/downtime) thời gian thực dành cho các Website, Web API, Database, và Redis chạy môi trường Production. Dự án được phát triển bằng **ASP.NET Core Web API (.NET 8)** kết hợp cơ sở dữ liệu **MongoDB**.

---

##  Tính năng cốt lõi

1. **Authentication (Xác thực bảo mật)**:
   - Đăng ký tài khoản mới & gửi mã kích hoạt OTP (giả lập gửi Email/Console).
   - Đăng nhập sinh mã bảo mật **JWT Bearer Token** với phân quyền vai trò chuyên nghiệp (`Admin` / `Viewer`).
2. **Project Management (Quản lý Dự án)**:
   - CRUD các dự án công nghệ của công ty.
   - Các API tạo, sửa, xóa dự án được bảo vệ nghiêm ngặt (chỉ vai trò `Admin` mới có quyền thực thi).
3. **Service Management (Quản lý Dịch vụ Giám sát)**:
   - Một dự án có thể cấu hình nhiều dịch vụ cần giám sát bên trong.
   - Hỗ trợ đa dạng các giao thức kiểm tra:
     - **Http / Https**: Kiểm tra Endpoint (Web App, API) trả về Response Code & tốc độ phản hồi.
     - **Tcp**: Bắt tay cổng TCP (phù hợp giám sát Database MongoDB, SQL, Redis, Mail Server,...).
     - **Ping**: Lệnh Ping truyền thống kiểm tra hạ tầng mạng IP/Port.
4. **Automated Background Monitor (Bộ quét chạy ngầm tự động)**:
   - Kế thừa lớp `BackgroundService` chạy ngầm song song với ứng dụng chính.
   - Định kỳ mỗi phút tự động lấy toàn bộ dịch vụ đang hoạt động (`IsActive = true`) và thực hiện kiểm tra đồng thời (Concurrency Tasks) để đạt hiệu năng cao nhất.
   - Đo thời gian phản hồi (Response Time tính bằng mili-giây) và lưu trữ lịch sử chi tiết vào MongoDB.
5. **Dashboard Analytics (Báo cáo trực quan)**:
   - API thống kê thời gian thực: Tổng số dịch vụ, số lượng Online/Offline tức thời.
   - Tự động tính toán tỷ lệ hoạt động ổn định **Uptime % trong 24 giờ qua** của toàn hệ thống.
   - Cung cấp danh sách lịch sử quét gần nhất (Recent Logs).

---

##  Công nghệ sử dụng

- **Framework chính**: ASP.NET Core Web API (Target Framework: `.NET 8.0`)
- **Cơ sở dữ liệu**: MongoDB (Sử dụng thư viện chính thức `MongoDB.Driver`)
- **Bảo mật**: JWT Bearer Authentication, BCrypt.Net (mã hóa mật khẩu)
- **Tài liệu hóa API**: Swagger / OpenAPI
- **Chạy nền**: BackgroundService (IHostedService)

---

##  Cấu trúc thư mục dự án

```text
backend/
├── Controllers/              # Xử lý các REST Endpoints (Auth, Project, Service, Dashboard)
├── Data/                     # Lớp kết nối dữ liệu (nếu có)
├── DTOs/                     # Lớp trung chuyển dữ liệu giữa Client và Server
│   ├── Auth/
│   ├── Project/
│   └── Service/
├── Model/                    # Định nghĩa cấu trúc các Entity lưu vào MongoDB (User, Project, Service, MonitorLog)
├── Properties/               # Cấu hình môi trường khởi chạy (launchSettings.json)
├── Repositories/             # Triển khai tầng truy cập DB sử dụng mẫu Generic Repository Pattern
│   └── Interfaces/
├── Services/                 # Tầng xử lý Logic Nghiệp vụ (Business Logic Layer)
│   ├── Interfaces/
│   ├── AuthService.cs
│   ├── ProjectService.cs
│   ├── ServiceService.cs
│   └── MonitoringService.cs  # Trái tim quét ngầm tự động của hệ thống
└── Program.cs                # Điểm khởi đầu ứng dụng, đăng ký DI và Middleware
```

---

## ⚙️ Cấu hình hệ thống

Mở file `appsettings.json` trong thư mục `backend/` và cập nhật thông số kết nối Database MongoDB và khóa bí mật JWT của bạn:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MonitoringSystemDb"
  },
  "Jwt": {
    "Secret": "SuperSecretKeyForMonitoringSystem2026SecureStringLongerThan32Bytes",
    "Issuer": "MonitoringSystem",
    "Audience": "MonitoringSystemUsers",
    "ExpiryInMinutes": 60
  }
}
```

---

## 🏃‍♂️ Hướng dẫn khởi chạy ứng dụng

Đảm bảo bạn đã cài đặt **.NET 8 SDK** và **MongoDB Server** đang chạy trên máy cục bộ của bạn.

1. Di chuyển vào thư mục backend:
   ```bash
   cd backend
   ```
2. Khôi phục các gói thư viện NuGet cần thiết:
   ```bash
   dotnet restore
   ```
3. Khởi chạy ứng dụng ở chế độ nhà phát triển (tự động tải lại khi sửa code):
   ```bash
   dotnet watch run
   ```
   dotnet publish -c Release -o ./publish
   dotnet ./publish/backend.dll --urls="http://0.0.0.0:5000

4. Khi terminal in ra cổng Port thành công, bạn có thể truy cập giao diện Swagger UI thử nghiệm tại:
   - **Swagger Link**: `http://localhost:5000/swagger/index.html` hoặc `https://localhost:5001/swagger/index.html`

---

## 📑 Hướng dẫn gọi API kiểm thử (Postman/Swagger)

Để kiểm thử bảo mật của hệ thống, các API liên quan đến **Project, Service, Dashboard** bắt buộc phải đính kèm tiêu đề **`Authorization: Bearer <Your_JWT_Token>`**.

### 1. Luồng Xác thực (Authentication)
* **Đăng ký tài khoản (`POST` /api/Auth/Register)**:
  ```json
  {
    "username": "admin_system",
    "email": "admin@company.com",
    "password": "Password123@"
  }
  ```
  *(Lấy mã OTP in tại cửa sổ Console của ứng dụng backend để kích hoạt).*
* **Xác thực OTP (`POST` /api/Auth/VerifyOtp)**:
  ```json
  {
    "username": "admin_system",
    "otpCode": "123456"
  }
  ```
* **Đăng nhập lấy Token JWT (`POST` /api/Auth/Login)**:
  ```json
  {
    "username": "admin_system",
    "password": "Password123@"
  }
  ```

### 2. Quản lý Dự án (`POST`, `GET`, `PUT`, `DELETE` /api/Project)
* **Tạo dự án mới (`POST` /api/Project)**:
  *(Chỉ tài khoản Admin thực hiện)*
  ```json
  {
    "name": "Hệ thống Bán Hàng E-Commerce",
    "description": "Giám sát hệ thống bán hàng chính",
    "status": "Active",
    "projectCode": "ECOMMERCE",
    "projectUrl": "https://company-shop.com",
    "repositoryUrl": "https://github.com/company/ecommerce"
  }
  ```

### 3. Cấu hình dịch vụ Giám sát (`POST`, `GET`, `PUT`, `DELETE` /api/Service)
* **Tạo dịch vụ giám sát HTTP (`POST` /api/Service)**:
  ```json
  {
    "projectId": "MÃ_ID_DỰ_ÁN_MONGODB",
    "serviceName": "Trang chủ Frontend",
    "ip": "gift-for-you.vercel.app",
    "port": 443,
    "protocol": "Https",
    "healthEndpoint": "/",
    "intervalSeconds": 60,
    "isActive": true
  }
  ```
* **Tạo dịch vụ giám sát TCP Database (`POST` /api/Service)**:
  ```json
  {
    "projectId": "MÃ_ID_DỰ_ÁN_MONGODB",
    "serviceName": "Cơ sở dữ liệu MongoDB",
    "ip": "127.0.0.1",
    "port": 27017,
    "protocol": "Tcp",
    "healthEndpoint": null,
    "intervalSeconds": 60,
    "isActive": true
  }
  ```

### 4. Báo cáo Dashboard (`GET` /api/Dashboard/Stats)
* **Xem dữ liệu trực quan (`GET` /api/Dashboard/Stats)**:
  Trả về tổng số dịch vụ, trạng thái tức thời và tỉ lệ Uptime 24h.
* **Xem log gần đây (`GET` /api/Dashboard/RecentLogs?limit=10)**:
  Trả về 10 lượt quét sức khỏe gần nhất lưu trong cơ sở dữ liệu.

---


