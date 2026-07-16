# 🐾 PetShop — Hướng dẫn cài đặt & chạy dự án

Website bán hàng & đặt lịch dịch vụ thú cưng, xây dựng theo mô hình **ASP.NET Core MVC (.NET 8)**. Ứng dụng phục vụ cả khách hàng (mua sắm, đặt lịch, đánh giá sản phẩm) lẫn quản trị viên (quản lý sản phẩm, đơn hàng, khuyến mãi, thống kê), và tích hợp chatbot AI tư vấn thú cưng bằng Google Gemini.

## 1. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM / Database | Entity Framework Core 8 + SQL Server (LocalDB) |
| Xác thực | Cookie Authentication + Google OAuth 2.0 |
| Giao diện | Razor View (.cshtml), Bootstrap 5, jQuery |
| Chatbot AI | Google Gemini API |
| Gửi email | SMTP (Gmail) |
| Giỏ hàng | ASP.NET Core Session |

## 2. Yêu cầu trước khi cài đặt

Cài sẵn các công cụ sau:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download)** — kiểm tra bằng `dotnet --version` (phải ra `8.x.x`).
- **SQL Server** hoặc **SQL Server LocalDB** (thường có sẵn khi cài Visual Studio, mục "Data storage and processing" khi cài đặt).
- **Visual Studio 2022** (khuyến nghị) hoặc **VS Code** + extension C#.
- (Tuỳ chọn) **EF Core CLI tools** nếu muốn tự chạy migration: `dotnet tool install --global dotnet-ef`.

## 3. Giải nén / Clone project

Nếu bạn có sẵn file `.zip`:

```bash
unzip PetShop-master.zip
cd PetShop-master
```

Hoặc clone trực tiếp từ Git:

```bash
git clone https://github.com/Nauht123/PetShop.git
cd PetShop
```

## 4. Tạo file cấu hình `appsettings.json`

Repo **không có sẵn** file `appsettings.json` ở thư mục gốc (chỉ có `appsettings.Development.json` chứa cấu hình log — không có secrets). Bạn cần tự tạo file `appsettings.json` cùng cấp với `PetShop.csproj`, nội dung như sau:

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
    "Model": "gemini-2.0-flash"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Ghi chú cho từng phần:

- **`ConnectionStrings:DefaultConnection`** — chuỗi kết nối SQL Server. Nếu dùng LocalDB thì giữ nguyên như trên; nếu dùng SQL Server thường thì đổi thành dạng `Server=.;Database=PetShopDB;User Id=sa;Password=...;TrustServerCertificate=True;`.
- **`Authentication:Google`** — dùng cho đăng nhập Google OAuth. Tạo tại [Google Cloud Console](https://console.cloud.google.com/) → *APIs & Services → Credentials → OAuth 2.0 Client ID* (loại Web application). Nếu chưa cần đăng nhập Google, có thể để trống chuỗi (`""`) nhưng **không được xoá** 2 khoá này vì `Program.cs` đọc trực tiếp và sẽ lỗi nếu thiếu.
- **`Smtp`** — dùng để gửi email quên mật khẩu. Với Gmail, dùng **App Password** (mật khẩu ứng dụng 16 ký tự), không dùng mật khẩu tài khoản thường (cần bật xác minh 2 bước trước).
- **`Gemini`** — API key cho chatbot AI, tạo tại [Google AI Studio](https://aistudio.google.com/app/apikey).

> Nếu chưa có Google OAuth / Gemini key ngay, bạn vẫn có thể chạy và test các chức năng khác (mua hàng, đặt lịch, admin...); chỉ tính năng đăng nhập Google và chatbot sẽ không hoạt động.

## 5. Cài đặt package

```bash
dotnet restore
```

## 6. Khởi tạo cơ sở dữ liệu

Ứng dụng tự gọi `Database.EnsureCreated()` và **seed sẵn dữ liệu mẫu** (tài khoản admin, danh mục, sản phẩm, dịch vụ) ngay khi khởi động lần đầu — **bạn không bắt buộc phải chạy migration thủ công**, chỉ cần đảm bảo SQL Server/LocalDB đang chạy và chuỗi kết nối ở bước 4 là đúng.

Nếu muốn tạo database bằng migration có sẵn trong thư mục `Migrations/` (cách làm chuẩn hơn cho môi trường thật), chạy:

```bash
dotnet ef database update
```

## 7. Chạy ứng dụng

```bash
dotnet run
```

Mở trình duyệt tại địa chỉ được in ra trong terminal (thường là `https://localhost:5001` hoặc `http://localhost:5000`).

Nếu dùng Visual Studio: mở file `PetShop.sln`, chọn cấu hình `https`, nhấn **F5**.

## 8. Đăng nhập tài khoản admin mặc định (được seed tự động)

- **Email:** `admin@petshop.com`
- **Mật khẩu:** `admin123`

> Nên đổi mật khẩu này ngay sau khi đăng nhập lần đầu nếu deploy lên môi trường công khai.

## 9. Cấu trúc thư mục chính

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
└── appsettings.json       # File bạn tự tạo ở bước 4 (không có sẵn trong repo)
```

## 10. Xử lý sự cố thường gặp

| Lỗi | Nguyên nhân / Cách khắc phục |
|---|---|
| `Cannot open database "PetShopDB"` | SQL Server/LocalDB chưa chạy, hoặc chuỗi kết nối sai. Kiểm tra lại `ConnectionStrings:DefaultConnection`. |
| Ứng dụng crash ngay khi start, báo lỗi liên quan `Authentication:Google:ClientId` | Thiếu khoá `Authentication:Google` trong `appsettings.json` — thêm vào dù để giá trị rỗng. |
| Không đăng nhập được bằng Google | Client ID/Secret sai, hoặc chưa cấu hình đúng **Authorized redirect URI** trong Google Cloud Console (dạng `https://localhost:5001/signin-google`). |
| Không gửi được email quên mật khẩu | Sai App Password Gmail, hoặc tài khoản Gmail chưa bật xác minh 2 bước để tạo App Password. |
| Chatbot không phản hồi | Sai hoặc thiếu `Gemini:ApiKey`, hoặc key đã hết hạn mức sử dụng. |
| Lỗi cổng bị chiếm khi `dotnet run` | Đổi cổng trong `Properties/launchSettings.json`, hoặc tắt tiến trình đang dùng cổng đó. |

## 11. ⚠️ Lưu ý bảo mật

`appsettings.json` chứa các secret nhạy cảm (Google OAuth Client Secret, mật khẩu ứng dụng Gmail, Gemini API key) nên **không được commit lên Git** — hãy đảm bảo file này nằm trong `.gitignore`.

- Nếu key cũ từng bị lộ công khai (ví dụ trong lịch sử commit), nên **thu hồi/tạo lại (rotate)** ngay cả khi file hiện tại đã được gỡ khỏi git:
  - Google Cloud Console → Credentials → tạo lại Client Secret.
  - Tài khoản Gmail → thu hồi App Password cũ, tạo cái mới.
  - Google AI Studio → thu hồi Gemini API key cũ, tạo key mới.
- Khi làm việc nhóm, nên thêm file mẫu `appsettings.Example.json` (không chứa giá trị thật) để người khác biết cần điền những trường nào.
- Với môi trường production, nên dùng [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (local) hoặc biến môi trường / Key Vault (server) thay vì lưu trực tiếp trong file cấu hình.

## 12. Giấy phép

Dự án phục vụ mục đích học tập.