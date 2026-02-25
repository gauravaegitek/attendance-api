// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using attendance_api.Models;

// namespace attendance_api.Data
// {
//     public class ApplicationDbContext : DbContext
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//             : base(options) { }

//         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         {
//             optionsBuilder.ConfigureWarnings(warnings =>
//                 warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
//         }

//         // ─── DbSets ───────────────────────────────────────────────────────
//         public DbSet<User> Users { get; set; } = null!;
//         public DbSet<Attendance> Attendances { get; set; } = null!;
//         public DbSet<Role> Roles { get; set; } = null!;
//         public DbSet<Holiday> Holidays { get; set; } = null!;
//         public DbSet<WFHRequest> WFHRequests { get; set; } = null!;
//         public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

//         // ✅ New DbSets
//         public DbSet<Notification> Notifications { get; set; } = null!;
//         public DbSet<Faq> Faqs { get; set; } = null!;
//         public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             // ─── User ─────────────────────────────────────────────────────
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

//                 // Profile fields
//                 entity.Property(e => e.Phone).HasMaxLength(20);
//                 entity.Property(e => e.Department).HasMaxLength(100);
//                 entity.Property(e => e.Designation).HasMaxLength(100);
//                 entity.Property(e => e.ProfilePhoto).HasMaxLength(500);
//                 entity.Property(e => e.Address).HasMaxLength(500);
//                 entity.Property(e => e.EmergencyContact).HasMaxLength(100);

//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
//                 entity.Property(e => e.IsActive).HasDefaultValue(true);

//                 entity.HasOne(e => e.RoleEntity)
//                     .WithMany(r => r.Users)
//                     .HasForeignKey(e => e.RoleId)
//                     .OnDelete(DeleteBehavior.SetNull);
//             });

//             // ─── Attendance ───────────────────────────────────────────────
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

//             // ─── Role ─────────────────────────────────────────────────────
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

//             // ─── Holiday ──────────────────────────────────────────────────
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

//             // ─── WFHRequest ───────────────────────────────────────────────
//             modelBuilder.Entity<WFHRequest>(entity =>
//             {
//                 entity.HasKey(e => e.WFHId);

//                 entity.HasIndex(e => new { e.UserId, e.WFHDate }).IsUnique();

//                 entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
//                 entity.Property(e => e.RejectionReason).HasMaxLength(500);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany(u => u.WFHRequests)
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ─── PerformanceReview ────────────────────────────────────────
//             modelBuilder.Entity<PerformanceReview>(entity =>
//             {
//                 entity.HasKey(e => e.ReviewId);

//                 entity.HasIndex(e => new { e.UserId, e.ReviewMonth, e.ReviewYear }).IsUnique();

//                 entity.Property(e => e.AttendanceScore).HasColumnType("decimal(5,2)");
//                 entity.Property(e => e.ManualScore).HasColumnType("decimal(5,2)");
//                 entity.Property(e => e.FinalScore).HasColumnType("decimal(5,2)");
//                 entity.Property(e => e.Grade).HasMaxLength(5).HasDefaultValue("C");
//                 entity.Property(e => e.ReviewerComments).HasMaxLength(1000);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany(u => u.PerformanceReviews)
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── Notification ───────────────────────────────────────────
//             modelBuilder.Entity<Notification>(entity =>
//             {
//                 entity.HasKey(e => e.NotificationId);

//                 entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
//                 entity.Property(e => e.Message).IsRequired();
//                 entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("alert");
//                 entity.Property(e => e.IsRead).HasDefaultValue(false);
//                 entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── Faq ────────────────────────────────────────────────────
//             modelBuilder.Entity<Faq>(entity =>
//             {
//                 entity.HasKey(e => e.FaqId);

//                 entity.Property(e => e.Question).IsRequired();
//                 entity.Property(e => e.Answer).IsRequired();
//                 entity.Property(e => e.Category).HasMaxLength(100).HasDefaultValue("general");
//                 entity.Property(e => e.SortOrder).HasDefaultValue(0);
//                 entity.Property(e => e.IsActive).HasDefaultValue(true);
//             });

//             // ✅ ─── ContactMessage ─────────────────────────────────────────
//             modelBuilder.Entity<ContactMessage>(entity =>
//             {
//                 entity.HasKey(e => e.ContactId);

//                 entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
//                 entity.Property(e => e.Message).IsRequired();
//                 entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("pending");
//                 entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ─── Seed: Roles ──────────────────────────────────────────────
//             modelBuilder.Entity<Role>().HasData(
//                 new Role
//                 {
//                     RoleId = 1,
//                     RoleName = "admin",
//                     Description = "System Admin",
//                     RequiresSelfie = false,
//                     IsActive = true,
//                     CreatedOn = new DateTime(2026, 2, 14, 0, 0, 0)
//                 }
//             );

//             // ─── Seed: Admin User ─────────────────────────────────────────
//             modelBuilder.Entity<User>().HasData(
//                 new User
//                 {
//                     UserId = 1,
//                     UserName = "Admin",
//                     Email = "admin@attendance.com",
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//                     Role = "admin",
//                     RoleId = 1,
//                     DeviceId = null,
//                     MacAddress = null,
//                     LastSeen = null,
//                     CreatedOn = new DateTime(2026, 2, 14, 0, 0, 0),
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

        // ─── DbSets ───────────────────────────────────────────────────────
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Holiday> Holidays { get; set; } = null!;
        public DbSet<WFHRequest> WFHRequests { get; set; } = null!;
        public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

        // ✅ New DbSets
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Faq> Faqs { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        // ✅ New DbSets — Leave, Location, DailyTask
        public DbSet<Leave> Leaves { get; set; } = null!;
        public DbSet<LocationTracking> LocationTrackings { get; set; } = null!;
        public DbSet<DailyTask> DailyTasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── User ─────────────────────────────────────────────────────
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

                // Profile fields
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Designation).HasMaxLength(100);
                entity.Property(e => e.ProfilePhoto).HasMaxLength(500);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.RoleEntity)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ─── Attendance ───────────────────────────────────────────────
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

            // ─── Role ─────────────────────────────────────────────────────
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

            // ─── Holiday ──────────────────────────────────────────────────
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

            // ─── WFHRequest ───────────────────────────────────────────────
            modelBuilder.Entity<WFHRequest>(entity =>
            {
                entity.HasKey(e => e.WFHId);

                entity.HasIndex(e => new { e.UserId, e.WFHDate }).IsUnique();

                entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.RejectionReason).HasMaxLength(500);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.WFHRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── PerformanceReview ────────────────────────────────────────
            modelBuilder.Entity<PerformanceReview>(entity =>
            {
                entity.HasKey(e => e.ReviewId);

                entity.HasIndex(e => new { e.UserId, e.ReviewMonth, e.ReviewYear }).IsUnique();

                entity.Property(e => e.AttendanceScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ManualScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.FinalScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Grade).HasMaxLength(5).HasDefaultValue("C");
                entity.Property(e => e.ReviewerComments).HasMaxLength(1000);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.PerformanceReviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ ─── Notification ───────────────────────────────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("alert");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ ─── Faq ────────────────────────────────────────────────────
            modelBuilder.Entity<Faq>(entity =>
            {
                entity.HasKey(e => e.FaqId);

                entity.Property(e => e.Question).IsRequired();
                entity.Property(e => e.Answer).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(100).HasDefaultValue("general");
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // ✅ ─── ContactMessage ─────────────────────────────────────────
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.ContactId);

                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ ─── Leave ──────────────────────────────────────────────────
            modelBuilder.Entity<Leave>(entity =>
            {
                entity.HasKey(e => e.LeaveId);

                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.AdminRemark).HasMaxLength(500);
                entity.Property(e => e.TotalDays).HasDefaultValue(1);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ ─── LocationTracking ───────────────────────────────────────
            modelBuilder.Entity<LocationTracking>(entity =>
            {
                entity.HasKey(e => e.TrackingId);

                entity.Property(e => e.CheckInAddress).HasMaxLength(500);
                entity.Property(e => e.CheckOutAddress).HasMaxLength(500);
                entity.Property(e => e.ClientName).HasMaxLength(200);
                entity.Property(e => e.ClientAddress).HasMaxLength(500);
                entity.Property(e => e.VisitPurpose).HasMaxLength(500);
                entity.Property(e => e.MeetingNotes).HasMaxLength(1000);
                entity.Property(e => e.Outcome).HasMaxLength(500);
                entity.Property(e => e.WorkType).HasMaxLength(20).HasDefaultValue("office");
                entity.Property(e => e.IsClientVisit).HasDefaultValue(false);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ ─── DailyTask ──────────────────────────────────────────────
            modelBuilder.Entity<DailyTask>(entity =>
            {
                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TaskDescription).HasMaxLength(1000);
                entity.Property(e => e.ProjectName).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("completed");
                entity.Property(e => e.HoursSpent).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Seed: Roles ──────────────────────────────────────────────
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

            // ─── Seed: Admin User ─────────────────────────────────────────
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