// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using attendance_api.Models;

// namespace attendance_api.Data
// {
//     public class ApplicationDbContext : DbContext
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//             : base(options)
//         {
//         }

//         // 👇 YEH ADD KARO
//         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         {
//             optionsBuilder.ConfigureWarnings(warnings =>
//                 warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
//         }

//         public DbSet<User> Users { get; set; }
//         public DbSet<Attendance> Attendances { get; set; }
//         public DbSet<Role> Roles { get; set; }
//         public DbSet<Holiday> Holidays { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             // User entity configuration
//             modelBuilder.Entity<User>(entity =>
//             {
//                 entity.HasKey(e => e.UserId);
//                 entity.HasIndex(e => e.Email).IsUnique();
//                 entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
//                 entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.DeviceId).HasMaxLength(255);
//                 entity.Property(e => e.MacAddress).HasMaxLength(255);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
//                 entity.Property(e => e.IsActive).HasDefaultValue(true);

//                 // Foreign key relationship with Role
//                 entity.HasOne(e => e.RoleEntity)
//                     .WithMany(r => r.Users)
//                     .HasForeignKey(e => e.RoleId)
//                     .OnDelete(DeleteBehavior.SetNull);
//             });

//             // Attendance entity configuration
//             modelBuilder.Entity<Attendance>(entity =>
//             {
//                 entity.HasKey(e => e.AttendanceId);
                
//                 entity.HasIndex(e => new { e.UserId, e.AttendanceDate }).IsUnique();

//                 entity.Property(e => e.AttendanceDate).IsRequired();
//                 entity.Property(e => e.InLatitude).HasColumnType("decimal(10,7)");
//                 entity.Property(e => e.InLongitude).HasColumnType("decimal(10,7)");
//                 entity.Property(e => e.OutLatitude).HasColumnType("decimal(10,7)");
//                 entity.Property(e => e.OutLongitude).HasColumnType("decimal(10,7)");
//                 entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
//                 entity.Property(e => e.InLocationAddress).HasMaxLength(500);
//                 entity.Property(e => e.OutLocationAddress).HasMaxLength(500);
//                 entity.Property(e => e.InSelfie).HasMaxLength(500);
//                 entity.Property(e => e.OutSelfie).HasMaxLength(500);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany(u => u.Attendances)
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // Role entity configuration
//             modelBuilder.Entity<Role>(entity =>
//             {
//                 entity.HasKey(e => e.RoleId);
//                 entity.HasIndex(e => e.RoleName).IsUnique();
//                 entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.Description).HasMaxLength(200);
//                 entity.Property(e => e.RequiresSelfie).HasDefaultValue(false);
//                 entity.Property(e => e.IsActive).HasDefaultValue(true);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
//             });

//             // Holiday entity configuration
//             modelBuilder.Entity<Holiday>(entity =>
//             {
//                 entity.HasKey(e => e.HolidayId);
//                 entity.HasIndex(e => e.HolidayDate).IsUnique();
//                 entity.Property(e => e.HolidayName).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.HolidayDate).IsRequired();
//                 entity.Property(e => e.Description).HasMaxLength(200);
//                 entity.Property(e => e.IsActive).HasDefaultValue(true);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
//             });

//             // 👇 SEED DATA FIX - RoleId = null (since FK is nullable with SetNull)
//             modelBuilder.Entity<User>().HasData(
//                 new User
//                 {
//                     UserId = 1,
//                     UserName = "Admin",
//                     Email = "admin@attendance.com",
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//                     Role = "admin",
//                     RoleId = null,  // 👈 YEH ADD KARO
//                     CreatedOn = DateTime.Now,
//                     IsActive = true
//                 }
//             );
//         }
//     }
// }










using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using attendance_api.Models;

namespace attendance_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Holiday> Holidays { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

                entity.Property(e => e.DeviceId).HasMaxLength(255);
                entity.Property(e => e.MacAddress).HasMaxLength(255);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.RoleEntity)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);

                entity.HasIndex(e => new { e.UserId, e.AttendanceDate }).IsUnique();

                entity.Property(e => e.AttendanceDate).IsRequired();
                entity.Property(e => e.InLatitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.InLongitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.OutLatitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.OutLongitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");

                entity.Property(e => e.InLocationAddress).HasMaxLength(500);
                entity.Property(e => e.OutLocationAddress).HasMaxLength(500);
                entity.Property(e => e.InSelfie).HasMaxLength(500);
                entity.Property(e => e.OutSelfie).HasMaxLength(500);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Attendances)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.RoleName).IsUnique();

                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.RequiresSelfie).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
            });

            // Holiday
            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasKey(e => e.HolidayId);
                entity.HasIndex(e => e.HolidayDate).IsUnique();

                entity.Property(e => e.HolidayName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HolidayDate).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
            });

            // ✅ Seed Roles (constant values)
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    RoleName = "admin",
                    Description = "System Admin",
                    RequiresSelfie = false,
                    IsActive = true,
                    CreatedOn = new DateTime(2026, 2, 14, 0, 0, 0)
                }
            );

            // ✅ Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    UserName = "Admin",
                    Email = "admin@attendance.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "admin",
                    RoleId = 1,
                    DeviceId = null,
                    MacAddress = null,
                    LastSeen = null,
                    CreatedOn = new DateTime(2026, 2, 14, 0, 0, 0),
                    IsActive = true
                }
            );
        }
    }
}
