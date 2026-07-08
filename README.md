# 🐾 PetShop

Website bán hàng & đặt lịch dịch vụ thú cưng, xây dựng theo mô hình **ASP.NET Core MVC**. Ứng dụng phục vụ cả khách hàng (mua sắm, đặt lịch, đánh giá sản phẩm) lẫn quản trị viên (quản lý sản phẩm, đơn hàng, khuyến mãi, thống kê), và tích hợp chatbot AI tư vấn thú cưng.

## Tính năng chính

### Khách hàng
- **Tài khoản**: đăng ký / đăng nhập bằng email + mật khẩu, đăng nhập bằng **Google OAuth**, quên/đặt lại mật khẩu qua email.
- **Sản phẩm**: xem danh sách theo danh mục, tìm kiếm, phân trang, xem chi tiết, đánh giá & chấm sao sản phẩm.
- **Giỏ hàng & đặt hàng**: thêm/sửa/xóa giỏ hàng (lưu trong Session), checkout, áp dụng mã khuyến mãi, xem lịch sử đơn hàng.
- **Đặt lịch dịch vụ**: đặt lịch tắm/spa, cắt tỉa lông, khám sức khỏe, tiêm phòng... và theo dõi trạng thái lịch hẹn.
- **Hồ sơ cá nhân**: cập nhật thông tin, địa chỉ, đổi mật khẩu, xem điểm tích lũy.
- **Chatbot AI**: trợ lý ảo tư vấn sản phẩm/dịch vụ, sử dụng Google **Gemini API**.
- **Trang chính sách**: vận chuyển, đổi trả, hàng chính hãng, hỗ trợ khách hàng.

### Quản trị (Admin)
- Dashboard thống kê doanh thu, đơn hàng.
- Quản lý sản phẩm (CRUD, đánh dấu "Hot"/"Bán chạy", upload ảnh).
- Quản lý danh mục sản phẩm.
- Quản lý đơn hàng & cập nhật trạng thái giao hàng.
- Quản lý lịch đặt dịch vụ.
- Quản lý mã khuyến mãi (giảm theo %, hoặc số tiền cố định, giới hạn số lần dùng, thời hạn áp dụng).
- Quản lý người dùng.

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM / Database | Entity Framework Core 8 + SQL Server (LocalDB) |
| Xác thực | Cookie Authentication + Google OAuth 2.0 |
| Giao diện | Razor View (.cshtml), Bootstrap 5, jQuery |
| Chatbot AI | Google Gemini API |
| Gửi email | SMTP (Gmail) |
| Quản lý trạng thái giỏ hàng | ASP.NET Core Session |

## Cấu trúc thư mục

```
PetShop/
├── Controllers/          # Controller phía khách hàng (Cart, Product, Order, Booking, Account, Chatbot...)
│   └── Admin/             # Controller khu vực quản trị (Product, Order, Category, User, Booking, KhuyenMai, Stat)
├── Data/                  # AppDbContext (EF Core)
├── Filters/               # AdminAuthFilter (bảo vệ khu vực admin), NavCategoryFilter (danh mục cho menu)
├── Helpers/               # CartHelper, PasswordHelper, EmailHelper
├── Migrations/            # EF Core Migrations
├── Models/                # Các entity: User, Product, Category, Order, OrderDetail, Booking, Service, Review, KhuyenMai...
├── Service/               # GeminiService (gọi Gemini API cho chatbot)
├── ViewModels/            # Các ViewModel dùng cho form (Login, Register, Checkout, Booking, Review...)
├── Views/                 # Giao diện Razor tương ứng với từng Controller
├── wwwroot/               # Static files: css, js, images, thư viện (Bootstrap, jQuery)
├── Program.cs             # Cấu hình app, DI, middleware, seed dữ liệu mẫu
└── appsettings.json       # Cấu hình kết nối DB, Google OAuth, SMTP, Gemini API key
```

## Yêu cầu cài đặt

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (hoặc SQL Server LocalDB, thường có sẵn khi cài Visual Studio)
- Visual Studio 2022 / VS Code

## Hướng dẫn chạy dự án

1. **Clone project**
   ```bash
   git clone https://github.com/Nauht123/PetShop.git
   cd PetShop
   ```

2. **Cấu hình `appsettings.json`**

   Cập nhật lại các thông tin sau bằng thông tin của riêng bạn (xem phần ⚠️ Bảo mật bên dưới):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PetShopDB;Trusted_Connection=True;"
     },
     "Authentication": {
       "Google": {
         "ClientId": "<your-google-client-id>",
         "ClientSecret": "<your-google-client-secret>"
       }
     },
     "Smtp": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "Username": "<your-email>",
       "Password": "<your-app-password>",
       "FromName": "PetShop"
     },
     "Gemini": {
       "ApiKey": "<your-gemini-api-key>",
       "Model": "gemini-2.5-flash"
     }
   }
   ```

3. **Cài package & tạo database**
   ```bash
   dotnet restore
   dotnet ef database update
   ```
   > Ứng dụng cũng tự gọi `EnsureCreated()` và seed sẵn dữ liệu mẫu (tài khoản admin, danh mục, sản phẩm, dịch vụ) khi khởi chạy lần đầu.

4. **Chạy ứng dụng**
   ```bash
   dotnet run
   ```
   Truy cập `https://localhost:5001` (hoặc cổng hiển thị trong terminal).

5. **Tài khoản admin mặc định** (được seed tự động)
   - Email: `admin@petshop.com`
   - Mật khẩu: `admin123`

## ⚠️ Lưu ý bảo mật

`appsettings.json` trong repo **không chứa secret thật** (Google OAuth Client Secret, mật khẩu ứng dụng Gmail, Gemini API key đã được gỡ bỏ) — bạn cần tự điền thông tin của mình vào các trường tương ứng để chạy được các tính năng liên quan (đăng nhập Google, gửi email, chatbot AI).

Vì các key này từng bị lộ công khai trước đó, nếu bạn chưa **thu hồi/tạo lại (rotate)** thì nên làm ngay:
- Google Cloud Console → Credentials → tạo lại Client Secret.
- Tài khoản Gmail → thu hồi App Password cũ, tạo cái mới.
- Google AI Studio → thu hồi Gemini API key cũ, tạo key mới.

Khuyến nghị: không commit secret thật vào `appsettings.json` nữa. Thay vào đó dùng [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) hoặc biến môi trường khi phát triển local:
```bash
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "your-real-key"
```

## Đóng góp

Mọi ý kiến đóng góp, báo lỗi hoặc pull request đều được hoan nghênh.

## Giấy phép

Dự án phục vụ mục đích học tập.
