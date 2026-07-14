using Microsoft.EntityFrameworkCore;
using PetShop.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<PetShop.Services.GeminiService>();

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<PetShop.Filters.NavCategoryFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<PetShop.Filters.NavCategoryFilter>();
});
builder.Services.AddScoped<PetShop.Filters.AdminAuthFilter>();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Google";
})
.AddCookie("Cookies")
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CallbackPath = "/signin-google";

    // Lấy thêm thông tin email và profile
    options.Scope.Add("email");
    options.Scope.Add("profile");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Home/Error404");
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // ← phải đứng trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Seed Admin mặc định
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // ── 1. SEED ADMIN ─────────────────────────────────────────────
    if (!db.Users.Any(u => u.VaiTro == "Admin"))
    {
        db.Users.Add(new PetShop.Models.User
        {
            HoTen = "Admin",
            Email = "admin@petshop.com",
            SoDienThoai = "0900000000",
            MatKhau = PetShop.Helpers.PasswordHelper.Hash("admin123"),
            VaiTro = "Admin",
            NgayTao = DateTime.Now
        });
        db.SaveChanges();
    }

    // ── 2. SEED USER MẪU ──────────────────────────────────────────
    if (!db.Users.Any(u => u.VaiTro == "KhachHang"))
    {
        db.Users.AddRange(
            new PetShop.Models.User
            {
                HoTen = "Nguyễn Văn An",
                Email = "an@gmail.com",
                SoDienThoai = "0901111111",
                MatKhau = PetShop.Helpers.PasswordHelper.Hash("123456"),
                VaiTro = "KhachHang",
                DiaChi1 = "123 Lê Lợi, Quận 1, TP.HCM",
                NgaySinh = new DateTime(1995, 3, 15),
                DiemTichLuy = 120,
                NgayTao = DateTime.Now.AddMonths(-6)
            },
            new PetShop.Models.User
            {
                HoTen = "Trần Thị Bình",
                Email = "binh@gmail.com",
                SoDienThoai = "0902222222",
                MatKhau = PetShop.Helpers.PasswordHelper.Hash("123456"),
                VaiTro = "KhachHang",
                DiaChi1 = "456 Nguyễn Huệ, Quận 1, TP.HCM",
                NgaySinh = new DateTime(1998, 7, 22),
                DiemTichLuy = 85,
                NgayTao = DateTime.Now.AddMonths(-4)
            },
            new PetShop.Models.User
            {
                HoTen = "Lê Minh Cường",
                Email = "cuong@gmail.com",
                SoDienThoai = "0903333333",
                MatKhau = PetShop.Helpers.PasswordHelper.Hash("123456"),
                VaiTro = "KhachHang",
                DiaChi1 = "789 Trần Hưng Đạo, Quận 5, TP.HCM",
                NgaySinh = new DateTime(1992, 11, 8),
                DiemTichLuy = 200,
                NgayTao = DateTime.Now.AddMonths(-8)
            },
            new PetShop.Models.User
            {
                HoTen = "Phạm Thị Dung",
                Email = "dung@gmail.com",
                SoDienThoai = "0904444444",
                MatKhau = PetShop.Helpers.PasswordHelper.Hash("123456"),
                VaiTro = "KhachHang",
                DiaChi1 = "321 Đinh Tiên Hoàng, Bình Thạnh, TP.HCM",
                NgaySinh = new DateTime(2000, 5, 30),
                DiemTichLuy = 45,
                NgayTao = DateTime.Now.AddMonths(-2)
            },
            new PetShop.Models.User
            {
                HoTen = "Hoàng Văn Em",
                Email = "em@gmail.com",
                SoDienThoai = "0905555555",
                MatKhau = PetShop.Helpers.PasswordHelper.Hash("123456"),
                VaiTro = "KhachHang",
                DiaChi1 = "654 Võ Văn Tần, Quận 3, TP.HCM",
                NgaySinh = new DateTime(1997, 9, 12),
                DiemTichLuy = 310,
                NgayTao = DateTime.Now.AddMonths(-12)
            }
        );
        db.SaveChanges();
    }

    // ── 3. SEED DANH MỤC ──────────────────────────────────────────
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new PetShop.Models.Category
            {
                TenDanhMuc = "Thức ăn chó",
                MoTa = "Các loại thức ăn dinh dưỡng dành cho chó"
            },
            new PetShop.Models.Category
            {
                TenDanhMuc = "Thức ăn mèo",
                MoTa = "Các loại thức ăn dinh dưỡng dành cho mèo"
            },
            new PetShop.Models.Category
            {
                TenDanhMuc = "Phụ kiện chó",
                MoTa = "Vòng cổ, dây dắt, áo, chuồng, đồ chơi cho chó"
            },
            new PetShop.Models.Category
            {
                TenDanhMuc = "Phụ kiện mèo",
                MoTa = "Nhà mèo, đồ chơi, cát vệ sinh, phụ kiện cho mèo"
            }
        );
        db.SaveChanges();
    }

    // ── 4. SEED SẢN PHẨM (40 sản phẩm) ───────────────────────────
    if (!db.Products.Any())
    {
        var cat1 = db.Categories.First(c => c.TenDanhMuc == "Thức ăn chó").Id;
        var cat2 = db.Categories.First(c => c.TenDanhMuc == "Thức ăn mèo").Id;
        var cat3 = db.Categories.First(c => c.TenDanhMuc == "Phụ kiện chó").Id;
        var cat4 = db.Categories.First(c => c.TenDanhMuc == "Phụ kiện mèo").Id;

        db.Products.AddRange(
            // ── THỨC ĂN CHÓ ──────────────────────────────────────
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Royal Canin cho chó 1kg",
                MoTa = "Thức ăn hạt cao cấp cho chó trưởng thành, giàu protein và vitamin, giúp chó khỏe mạnh và bóng lông.",
                Gia = 185000,
                SoLuongKho = 50,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Royal+Canin",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Pate Whiskas cho chó 400g",
                MoTa = "Pate thơm ngon bổ dưỡng cho chó mọi lứa tuổi, thành phần tự nhiên, dễ tiêu hóa.",
                Gia = 45000,
                SoLuongKho = 100,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Pate+Cho",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Pedigree cho chó con 1.5kg",
                MoTa = "Dinh dưỡng đặc biệt cho chó con từ 2-12 tháng tuổi, hỗ trợ phát triển xương và não bộ.",
                Gia = 145000,
                SoLuongKho = 60,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Pedigree+Con"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Ganador cho chó lớn 3kg",
                MoTa = "Dành cho chó lớn trên 25kg, công thức đặc biệt hỗ trợ khớp và cơ bắp khỏe mạnh.",
                Gia = 320000,
                SoLuongKho = 35,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Ganador",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Bánh thưởng Treats cho chó 200g",
                MoTa = "Bánh thưởng giòn xương, vị gà thơm ngon, dùng để huấn luyện hoặc thưởng cho chó.",
                Gia = 65000,
                SoLuongKho = 80,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Treats+Cho",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Xương gặm tự nhiên cho chó size M",
                MoTa = "Xương gặm tự nhiên giúp làm sạch răng và giải trí cho chó, an toàn 100% tự nhiên.",
                Gia = 55000,
                SoLuongKho = 45,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Xuong+Gam"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn ướt Alpo cho chó 700g",
                MoTa = "Thức ăn ướt vị bò và rau củ, giàu dinh dưỡng, phù hợp cho chó kén ăn.",
                Gia = 75000,
                SoLuongKho = 55,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Alpo"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Sữa cho chó con Beaphar 200g",
                MoTa = "Sữa bột thay thế sữa mẹ dành cho chó con mới sinh, đầy đủ dưỡng chất thiết yếu.",
                Gia = 220000,
                SoLuongKho = 25,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Sua+Cho+Con"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt ProPlan cho chó nhỏ 800g",
                MoTa = "Dành riêng cho giống chó nhỏ dưới 10kg, hạt nhỏ dễ nhai, giàu omega-3.",
                Gia = 195000,
                SoLuongKho = 40,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=ProPlan"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Snack que thưởng Dentastix cho chó",
                MoTa = "Que nhai vệ sinh răng miệng hàng ngày, giảm 80% mảng bám và cao răng.",
                Gia = 85000,
                SoLuongKho = 70,
                CategoryId = cat1,
                HinhAnh = "https://placehold.co/400x400/4A90D9/white?text=Dentastix",
                IsBanChay = true
            },

            // ── THỨC ĂN MÈO ──────────────────────────────────────
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Me-O cho mèo 1.2kg",
                MoTa = "Hạt khô đầy đủ dinh dưỡng cho mèo trưởng thành, giúp lông bóng mượt và tiêu hóa tốt.",
                Gia = 95000,
                SoLuongKho = 80,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Me-O",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Pate Whiskas cho mèo 85g",
                MoTa = "Pate mềm thơm cho mèo mọi lứa tuổi, vị cá hồi và tôm, kích thích ăn ngon.",
                Gia = 18000,
                SoLuongKho = 200,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Pate+Meo",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Royal Canin cho mèo con 400g",
                MoTa = "Dành cho mèo con từ 4-12 tháng, hỗ trợ hệ miễn dịch và phát triển xương.",
                Gia = 165000,
                SoLuongKho = 45,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=RC+Meo+Con",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Catsrang cho mèo 1kg",
                MoTa = "Thức ăn hạt Hàn Quốc chất lượng cao, vị cá ngừ, phù hợp mèo mọi lứa tuổi.",
                Gia = 88000,
                SoLuongKho = 90,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Catsrang"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Snack mèo Temptations vị gà 85g",
                MoTa = "Bánh thưởng mèo giòn bên ngoài, mềm bên trong, mèo nào cũng mê.",
                Gia = 48000,
                SoLuongKho = 120,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Temptations",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn ướt Fancy Feast cho mèo 85g",
                MoTa = "Thức ăn ướt cao cấp vị hải sản, mèo khó tính cũng thích, giàu protein.",
                Gia = 32000,
                SoLuongKho = 150,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Fancy+Feast"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Cát vệ sinh mèo Biokats 10L",
                MoTa = "Cát đất sét khử mùi siêu tốt, vón cục chắc, dễ vệ sinh, an toàn cho mèo.",
                Gia = 185000,
                SoLuongKho = 30,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Biokats"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Snack que tươi Inaba Churu cho mèo",
                MoTa = "Snack que dạng lỏng vị gà và cá ngừ, bổ sung nước và dinh dưỡng cho mèo.",
                Gia = 25000,
                SoLuongKho = 180,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Churu",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Sữa cho mèo CatSure 200ml",
                MoTa = "Sữa không lactose dành riêng cho mèo, bổ sung dinh dưỡng và tăng đề kháng.",
                Gia = 55000,
                SoLuongKho = 60,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=CatSure"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Thức ăn hạt Nutrience cho mèo triệt sản 1kg",
                MoTa = "Dành riêng cho mèo đã triệt sản, kiểm soát cân nặng và hỗ trợ tiết niệu.",
                Gia = 210000,
                SoLuongKho = 35,
                CategoryId = cat2,
                HinhAnh = "https://placehold.co/400x400/E8A838/white?text=Nutrience"
            },

            // ── PHỤ KIỆN CHÓ ─────────────────────────────────────
            new PetShop.Models.Product
            {
                TenSanPham = "Vòng cổ da cho chó size M",
                MoTa = "Vòng cổ da bền đẹp cho chó cỡ vừa, có khóa điều chỉnh kích thước tiện lợi.",
                Gia = 75000,
                SoLuongKho = 30,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Vong+Co+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Dây dắt chó 1.5m",
                MoTa = "Dây dắt chắc chắn, thoải mái khi dắt chó đi dạo, chất liệu nylon cao cấp.",
                Gia = 55000,
                SoLuongKho = 40,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Day+Dat+Cho",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Áo mưa cho chó size S",
                MoTa = "Áo mưa chống thấm nước hoàn toàn, thiết kế dễ mặc, có mũ trùm đầu.",
                Gia = 120000,
                SoLuongKho = 20,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Ao+Mua+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Chuồng sắt cho chó size L",
                MoTa = "Chuồng sắt chắc chắn, thoáng mát, dễ vệ sinh, có khay hứng phân tiện lợi.",
                Gia = 850000,
                SoLuongKho = 10,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Chuong+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Bóng đồ chơi cho chó cao su",
                MoTa = "Bóng cao su tự nhiên an toàn, đàn hồi tốt, phù hợp cho chó gặm và chơi.",
                Gia = 35000,
                SoLuongKho = 60,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Bong+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Nệm ngủ cho chó size M",
                MoTa = "Nệm êm ái, chất liệu nhung mềm mịn, giữ ấm tốt, có thể giặt máy được.",
                Gia = 280000,
                SoLuongKho = 15,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Nem+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Bàn chải đánh răng cho chó",
                MoTa = "Bàn chải đánh răng 3 đầu cho chó, làm sạch toàn diện, lông mềm không gây đau.",
                Gia = 45000,
                SoLuongKho = 50,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Ban+Chai+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Lược chải lông cho chó",
                MoTa = "Lược chải lông chuyên dụng, loại bỏ lông rụng hiệu quả, massage da đầu nhẹ nhàng.",
                Gia = 65000,
                SoLuongKho = 45,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Luoc+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Balo đựng chó du lịch",
                MoTa = "Balo trong suốt chở chó đi chơi, thoáng khí, chịu tải đến 6kg, thiết kế thời trang.",
                Gia = 450000,
                SoLuongKho = 12,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Balo+Cho"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Sữa tắm cho chó Bio-Groom 355ml",
                MoTa = "Sữa tắm thảo dược tự nhiên, khử mùi lâu dài, dưỡng lông bóng mượt, an toàn cho da.",
                Gia = 155000,
                SoLuongKho = 35,
                CategoryId = cat3,
                HinhAnh = "https://placehold.co/400x400/5CB85C/white?text=Sua+Tam+Cho"
            },

            // ── PHỤ KIỆN MÈO ─────────────────────────────────────
            new PetShop.Models.Product
            {
                TenSanPham = "Nhà mèo lông xù size L",
                MoTa = "Nhà lông ấm áp, mèo thích nằm, chất liệu plush siêu mềm, có thể giặt được.",
                Gia = 320000,
                SoLuongKho = 15,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Nha+Meo",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Cần câu đồ chơi cho mèo",
                MoTa = "Đồ chơi cần câu lông vũ kích thích mèo vận động, giúp mèo giảm căng thẳng.",
                Gia = 35000,
                SoLuongKho = 60,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Can+Cau+Meo",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Trụ cào móng cho mèo 60cm",
                MoTa = "Trụ cào móng bọc đay tự nhiên, bền chắc, giúp mèo mài móng và giảm stress.",
                Gia = 185000,
                SoLuongKho = 20,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Tru+Cao+Mong",
                IsHot = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Vòng cổ chống bọ chét cho mèo",
                MoTa = "Vòng cổ xua đuổi bọ chét và ve trong 8 tháng, an toàn không độc hại.",
                Gia = 95000,
                SoLuongKho = 40,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Vong+Bo+Chet"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Khay vệ sinh mèo có lưới lọc",
                MoTa = "Khay vệ sinh có lưới lọc thông minh, ngăn cát rơi vãi, dễ vệ sinh hàng ngày.",
                Gia = 145000,
                SoLuongKho = 25,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Khay+Ve+Sinh"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Đường hầm đồ chơi cho mèo",
                MoTa = "Đường hầm vải gấp gọn được, mèo thích chui vào chơi, có lỗ để mèo thò tay.",
                Gia = 115000,
                SoLuongKho = 30,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Duong+Ham+Meo"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Lược chải lông mèo tự làm sạch",
                MoTa = "Lược chải lông có nút bấm tự đẩy lông ra, tiện lợi, không cần tay nhổ lông.",
                Gia = 78000,
                SoLuongKho = 55,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Luoc+Meo",
                IsBanChay = true
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Sữa tắm cho mèo Fay 300ml",
                MoTa = "Sữa tắm dịu nhẹ cho mèo, hương thơm nhẹ nhàng, không cay mắt, dưỡng ẩm tốt.",
                Gia = 125000,
                SoLuongKho = 40,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Sua+Tam+Meo"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Chuồng mèo 3 tầng",
                MoTa = "Chuồng mèo 3 tầng rộng rãi, có bục nhảy và ống chui, phù hợp nuôi nhiều mèo.",
                Gia = 1250000,
                SoLuongKho = 5,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Chuong+Meo"
            },
            new PetShop.Models.Product
            {
                TenSanPham = "Bộ đồ chơi mèo 10 món",
                MoTa = "Bộ đồ chơi đa dạng gồm chuột bông, bóng chuông, cần câu, phù hợp mèo mọi lứa tuổi.",
                Gia = 165000,
                SoLuongKho = 22,
                CategoryId = cat4,
                HinhAnh = "https://placehold.co/400x400/D9534F/white?text=Bo+Do+Choi+Meo"
            }
        );
        db.SaveChanges();
    }

    // ── 5. SEED DỊCH VỤ ───────────────────────────────────────────
    if (!db.Services.Any())
    {
        db.Services.AddRange(
            new PetShop.Models.Service
            {
                TenDichVu = "Tắm & Sấy lông",
                MoTa = "Tắm sạch, sấy khô, khử mùi cho thú cưng",
                Gia = 150000,
                ThoiGianPhut = 60
            },
            new PetShop.Models.Service
            {
                TenDichVu = "Cắt tỉa lông",
                MoTa = "Cắt tỉa lông theo yêu cầu, tạo kiểu chuyên nghiệp",
                Gia = 200000,
                ThoiGianPhut = 90
            },
            new PetShop.Models.Service
            {
                TenDichVu = "Tắm + Cắt tỉa lông (Combo)",
                MoTa = "Tắm sạch và cắt tỉa lông trọn gói",
                Gia = 300000,
                ThoiGianPhut = 120
            },
            new PetShop.Models.Service
            {
                TenDichVu = "Khám sức khỏe tổng quát",
                MoTa = "Kiểm tra sức khỏe, tư vấn dinh dưỡng cho thú cưng",
                Gia = 250000,
                ThoiGianPhut = 45
            },
            new PetShop.Models.Service
            {
                TenDichVu = "Tiêm phòng",
                MoTa = "Tiêm vaccine phòng bệnh định kỳ",
                Gia = 350000,
                ThoiGianPhut = 30
            }
        );
        db.SaveChanges();
    }

    // ── 6. SEED REVIEWS ───────────────────────────────────────────
    if (!db.Reviews.Any())
    {
        var products = db.Products.ToList();
        var users = db.Users.Where(u => u.VaiTro == "KhachHang").ToList();

        var reviewContents = new Dictionary<int, List<string>>
        {
            [5] = new List<string>
            {
                "Sản phẩm tuyệt vời, bé nhà mình rất thích!",
                "Chất lượng rất tốt, đóng gói cẩn thận, giao hàng nhanh.",
                "Mua lần 2 rồi, vẫn hài lòng như lần đầu. Sẽ tiếp tục ủng hộ!",
                "Bé ăn rất ngon miệng, không bị bỏ thừa. Rất đáng tiền!",
                "Shop tư vấn nhiệt tình, sản phẩm đúng mô tả. 5 sao!"
            },
            [4] = new List<string>
            {
                "Sản phẩm tốt, bé nhà mình thích ăn. Giao hàng hơi chậm.",
                "Chất lượng ổn, giá hợp lý. Sẽ mua lại.",
                "Hàng đúng mô tả, bao bì đẹp. Nhưng giao hơi lâu một chút.",
                "Dùng được, bé thích. Mình sẽ order thêm lần sau.",
                "Sản phẩm khá tốt, chỉ tiếc là hết hàng nhanh quá."
            },
            [3] = new List<string>
            {
                "Sản phẩm bình thường, không có gì đặc biệt.",
                "Bé nhà mình ăn được nhưng không hào hứng lắm.",
                "Tạm ổn, nhưng mình nghĩ có sản phẩm khác tốt hơn.",
                "Giao hàng chậm, bao bì hơi xộc xệch. Sản phẩm thì ổn.",
                "Chất lượng trung bình, giá hơi cao so với chất lượng."
            }
        };

        var random = new Random(42);
        var starPool = new[] { 5, 5, 5, 4, 4, 3 };

        foreach (var product in products)
        {
            int reviewCount = random.Next(3, 6);
            var shuffledUsers = users
                .OrderBy(_ => random.Next())
                .Take(reviewCount)
                .ToList();

            var baseDate = DateTime.Now.AddDays(-random.Next(10, 90));

            for (int i = 0; i < shuffledUsers.Count; i++)
            {
                int soSao = starPool[random.Next(starPool.Length)];
                string noiDung = reviewContents[soSao]
                    [random.Next(reviewContents[soSao].Count)];

                db.Reviews.Add(new PetShop.Models.Review
                {
                    ProductId = product.Id,
                    UserId = shuffledUsers[i].Id,
                    SoSao = soSao,
                    NoiDung = noiDung,
                    NgayTao = baseDate.AddDays(i * random.Next(1, 5))
                });
            }
        }
        db.SaveChanges();
    }

    // ── 7. SEED KHUYẾN MÃI ────────────────────────────────────────
    if (!db.KhuyenMais.Any())
    {
        db.KhuyenMais.AddRange(
            new PetShop.Models.KhuyenMai
            {
                MaCode = "WELCOME20",
                MoTa = "Giảm 20% cho đơn hàng đầu tiên",
                LoaiGiam = 1,
                GiaTriGiam = 20,
                DonHangToiThieu = 100000,
                GiamToiDa = 100000,
                SoLuong = 100,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(3),
                IsActive = true
            },
            new PetShop.Models.KhuyenMai
            {
                MaCode = "FREESHIP",
                MoTa = "Giảm 30.000đ phí ship cho đơn từ 200.000đ",
                LoaiGiam = 2,
                GiaTriGiam = 30000,
                DonHangToiThieu = 200000,
                GiamToiDa = 30000,
                SoLuong = 200,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                IsActive = true
            },
            new PetShop.Models.KhuyenMai
            {
                MaCode = "PETLOVE50",
                MoTa = "Giảm 50.000đ cho đơn từ 500.000đ",
                LoaiGiam = 2,
                GiaTriGiam = 50000,
                DonHangToiThieu = 500000,
                GiamToiDa = 50000,
                SoLuong = 50,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(2),
                IsActive = true
            },
            new PetShop.Models.KhuyenMai
            {
                MaCode = "SUMMER15",
                MoTa = "Giảm 15% mùa hè, tối đa 80.000đ",
                LoaiGiam = 1,
                GiaTriGiam = 15,
                DonHangToiThieu = 150000,
                GiamToiDa = 80000,
                SoLuong = 150,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(2),
                IsActive = true
            }
        );
        db.SaveChanges();
    }
}


app.Run();