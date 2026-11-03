using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<Account>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets cho các thực thể nhà trọ
        public DbSet<House> Houses => Set<House>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bắt buộc gọi base để IdentityDbContext tạo các bảng Identity (AspNetUsers, AspNetRoles...)
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. CẤU HÌNH THỰC THỂ ACCOUNT (Người dùng / IdentityUser)
            // ==========================================
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(a => a.CitizenId)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(a => a.Address)
                    .HasMaxLength(300);

                entity.Property(a => a.Gender)
                    .HasMaxLength(10);

                entity.Property(a => a.AvatarUrl)
                    .HasMaxLength(500);

                // Index để tra cứu nhanh khách thuê theo số CCCD
                entity.HasIndex(a => a.CitizenId);
            });

            // ==========================================
            // 2. CẤU HÌNH THỰC THỂ HOUSE (Nhà trọ / Dãy trọ)
            // ==========================================
            modelBuilder.Entity<House>(entity =>
            {
                entity.HasKey(h => h.HouseId);

                entity.Property(h => h.HouseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(h => h.Address)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(h => h.Description)
                    .HasMaxLength(500);

                // Cấu hình độ chính xác tiền tệ (18, 2)
                entity.Property(h => h.ElectricityUnitPrice).HasPrecision(18, 2);
                entity.Property(h => h.WaterUnitPrice).HasPrecision(18, 2);
                entity.Property(h => h.InternetUnitPrice).HasPrecision(18, 2);
                entity.Property(h => h.CleaningUnitPrice).HasPrecision(18, 2);
                entity.Property(h => h.ParkingUnitPrice).HasPrecision(18, 2);

                // Quan hệ: House thuộc về Owner (Account)
                entity.HasOne(h => h.Owner)
                    .WithMany(a => a.Houses)
                    .HasForeignKey(h => h.OwnerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ: House có nhiều Rooms
                entity.HasMany(h => h.Rooms)
                    .WithOne(r => r.House)
                    .HasForeignKey(r => r.HouseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // 3. CẤU HÌNH THỰC THỂ ROOM (Phòng trọ)
            // ==========================================
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.RoomId);

                entity.Property(r => r.RoomNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.Price).HasPrecision(18, 2);

                // Unique constraint: Trong cùng 1 nhà trọ, số phòng không được trùng nhau
                entity.HasIndex(r => new { r.HouseId, r.RoomNumber })
                    .IsUnique();

                // Quan hệ: Room có nhiều Contracts
                entity.HasMany(r => r.Contracts)
                    .WithOne(c => c.Room)
                    .HasForeignKey(c => c.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: Room có nhiều MeterReadings
                entity.HasMany(r => r.MeterReadings)
                    .WithOne(m => m.Room)
                    .HasForeignKey(m => m.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: Room có nhiều Invoices
                entity.HasMany(r => r.Invoices)
                    .WithOne(i => i.Room)
                    .HasForeignKey(i => i.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 4. CẤU HÌNH THỰC THỂ CONTRACT (Hợp đồng thuê)
            // ==========================================
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.ContractId);

                entity.Property(c => c.ContractNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(c => c.ContractNumber)
                    .IsUnique();

                // Tiền tệ & chỉ số
                entity.Property(c => c.RentalPrice).HasPrecision(18, 2);
                entity.Property(c => c.DepositAmount).HasPrecision(18, 2);
                entity.Property(c => c.DepositRefunded).HasPrecision(18, 2);
                entity.Property(c => c.InitialElectricityIndex).HasPrecision(18, 2);
                entity.Property(c => c.InitialWaterIndex).HasPrecision(18, 2);
                entity.Property(c => c.ElectricityUnitPrice).HasPrecision(18, 2);
                entity.Property(c => c.WaterUnitPrice).HasPrecision(18, 2);
                entity.Property(c => c.InternetUnitPrice).HasPrecision(18, 2);
                entity.Property(c => c.CleaningUnitPrice).HasPrecision(18, 2);
                entity.Property(c => c.ParkingUnitPrice).HasPrecision(18, 2);

                entity.Property(c => c.ContractImageUrl)
                    .HasMaxLength(500);

                entity.Property(c => c.Note)
                    .HasMaxLength(1000);

                // Quan hệ: Contract thuộc về Khách thuê (Account)
                entity.HasOne(c => c.Account)
                    .WithMany(a => a.Contracts)
                    .HasForeignKey(c => c.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: Contract có nhiều Invoices
                entity.HasMany(c => c.Invoices)
                    .WithOne(i => i.Contract)
                    .HasForeignKey(i => i.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: Contract có nhiều MeterReadings
                entity.HasMany(c => c.MeterReadings)
                    .WithOne(m => m.Contract)
                    .HasForeignKey(m => m.ContractId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ: Contract có nhiều Payments
                entity.HasMany(c => c.Payments)
                    .WithOne(p => p.Contract)
                    .HasForeignKey(p => p.ContractId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================================
            // 5. CẤU HÌNH THỰC THỂ METER READING (Chốt số điện nước)
            // ==========================================
            modelBuilder.Entity<MeterReading>(entity =>
            {
                entity.HasKey(m => m.MeterReadingId);

                entity.Property(m => m.OldElectricityIndex).HasPrecision(18, 2);
                entity.Property(m => m.NewElectricityIndex).HasPrecision(18, 2);
                entity.Property(m => m.OldWaterIndex).HasPrecision(18, 2);
                entity.Property(m => m.NewWaterIndex).HasPrecision(18, 2);

                entity.Property(m => m.ElectricityImageUrl).HasMaxLength(500);
                entity.Property(m => m.WaterImageUrl).HasMaxLength(500);
                entity.Property(m => m.Note).HasMaxLength(500);

                // Unique: Mỗi phòng chỉ chốt số 1 lần trong 1 tháng
                entity.HasIndex(m => new { m.RoomId, m.Month, m.Year })
                    .IsUnique();
            });

            // ==========================================
            // 6. CẤU HÌNH THỰC THỂ INVOICE (Hóa đơn hàng tháng)
            // ==========================================
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.InvoiceId);

                entity.Property(i => i.InvoiceCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(i => i.InvoiceCode)
                    .IsUnique();

                // Unique: Mỗi phòng chỉ tạo 1 hóa đơn cho 1 kỳ (tháng/năm)
                entity.HasIndex(i => new { i.RoomId, i.Month, i.Year })
                    .IsUnique();

                // Các trường tiền tệ & lượng dùng
                entity.Property(i => i.RoomPrice).HasPrecision(18, 2);
                entity.Property(i => i.ElectricityUsage).HasPrecision(18, 2);
                entity.Property(i => i.ElectricityUnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.ElectricityCost).HasPrecision(18, 2);
                entity.Property(i => i.WaterUsage).HasPrecision(18, 2);
                entity.Property(i => i.WaterUnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.WaterCost).HasPrecision(18, 2);
                entity.Property(i => i.InternetCost).HasPrecision(18, 2);
                entity.Property(i => i.CleaningCost).HasPrecision(18, 2);
                entity.Property(i => i.ParkingUnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.ParkingCost).HasPrecision(18, 2);
                entity.Property(i => i.OtherFees).HasPrecision(18, 2);
                entity.Property(i => i.DiscountAmount).HasPrecision(18, 2);
                entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
                entity.Property(i => i.PaidAmount).HasPrecision(18, 2);

                entity.Property(i => i.OtherFeesDescription).HasMaxLength(300);
                entity.Property(i => i.DiscountReason).HasMaxLength(300);
                entity.Property(i => i.PaymentMethod).HasMaxLength(50);
                entity.Property(i => i.Note).HasMaxLength(500);

                // Quan hệ: Invoice thuộc về Account khách thuê
                entity.HasOne(i => i.Account)
                    .WithMany(a => a.Invoices)
                    .HasForeignKey(i => i.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: Invoice gắn với MeterReading
                entity.HasOne(i => i.MeterReading)
                    .WithMany()
                    .HasForeignKey(i => i.MeterReadingId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ: Invoice có nhiều Payments
                entity.HasMany(i => i.Payments)
                    .WithOne(p => p.Invoice)
                    .HasForeignKey(p => p.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // 7. CẤU HÌNH THỰC THỂ PAYMENT (Giao dịch thanh toán)
            // ==========================================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                entity.Property(p => p.PaymentCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(p => p.PaymentCode)
                    .IsUnique();

                entity.Property(p => p.TransactionId)
                    .HasMaxLength(100);

                entity.Property(p => p.Amount).HasPrecision(18, 2);

                entity.Property(p => p.ProofImageUrl).HasMaxLength(500);
                entity.Property(p => p.Note).HasMaxLength(500);

                // Quan hệ: Payment thuộc về Account thanh toán
                entity.HasOne(p => p.Account)
                    .WithMany(a => a.Payments)
                    .HasForeignKey(p => p.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
