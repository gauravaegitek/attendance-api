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

//         // ✅ New DbSets — Leave, Location, DailyTask
//         public DbSet<Leave> Leaves { get; set; } = null!;
//         public DbSet<LocationTracking> LocationTrackings { get; set; } = null!;
//         public DbSet<DailyTask> DailyTasks { get; set; } = null!;

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

//             // ✅ ─── Leave ──────────────────────────────────────────────────
//             modelBuilder.Entity<Leave>(entity =>
//             {
//                 entity.HasKey(e => e.LeaveId);

//                 entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
//                 entity.Property(e => e.AdminRemark).HasMaxLength(500);
//                 entity.Property(e => e.TotalDays).HasDefaultValue(1);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── LocationTracking ───────────────────────────────────────
//             modelBuilder.Entity<LocationTracking>(entity =>
//             {
//                 entity.HasKey(e => e.TrackingId);

//                 entity.Property(e => e.CheckInAddress).HasMaxLength(500);
//                 entity.Property(e => e.CheckOutAddress).HasMaxLength(500);
//                 entity.Property(e => e.ClientName).HasMaxLength(200);
//                 entity.Property(e => e.ClientAddress).HasMaxLength(500);
//                 entity.Property(e => e.VisitPurpose).HasMaxLength(500);
//                 entity.Property(e => e.MeetingNotes).HasMaxLength(1000);
//                 entity.Property(e => e.Outcome).HasMaxLength(500);
//                 entity.Property(e => e.WorkType).HasMaxLength(20).HasDefaultValue("office");
//                 entity.Property(e => e.IsClientVisit).HasDefaultValue(false);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── DailyTask ──────────────────────────────────────────────
//             modelBuilder.Entity<DailyTask>(entity =>
//             {
//                 entity.HasKey(e => e.TaskId);

//                 entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(200);
//                 entity.Property(e => e.TaskDescription).HasMaxLength(1000);
//                 entity.Property(e => e.ProjectName).HasMaxLength(200);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("completed");
//                 entity.Property(e => e.HoursSpent).HasColumnType("decimal(5,2)").HasDefaultValue(0);
//                 entity.Property(e => e.Remarks).HasMaxLength(500);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

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

//             // // ─── Seed: Admin User ─────────────────────────────────────────
//             // modelBuilder.Entity<User>().HasData(
//             //     new User
//             //     {
//             //         UserId = 1,
//             //         UserName = "Admin",
//             //         Email = "admin@attendance.com",
//             //         PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//             //         Role = "admin",
//             //         RoleId = 1,
//             //         DeviceId = null,
//             //         MacAddress = null,
//             //         LastSeen = null,
//             //         CreatedOn = new DateTime(2026, 2, 14, 0, 0, 0),
//             //         IsActive = true
//             //     }
//             // );
//         }
//     }
// }







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
//         public DbSet<User>              Users              { get; set; } = null!;
//         public DbSet<Attendance>        Attendances        { get; set; } = null!;
//         public DbSet<Role>              Roles              { get; set; } = null!;
//         public DbSet<Holiday>           Holidays           { get; set; } = null!;
//         public DbSet<WFHRequest>        WFHRequests        { get; set; } = null!;
//         public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

//         // ✅ New DbSets
//         public DbSet<Notification>   Notifications   { get; set; } = null!;
//         public DbSet<Faq>            Faqs            { get; set; } = null!;
//         public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

//         // ✅ New DbSets — Leave, Location, DailyTask
//         public DbSet<Leave>            Leaves            { get; set; } = null!;
//         public DbSet<LocationTracking> LocationTrackings { get; set; } = null!;
//         public DbSet<DailyTask>        DailyTasks        { get; set; } = null!;

//         // ✅ NEW — Login History
//         public DbSet<UserLoginHistory> UserLoginHistories { get; set; } = null!;

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

//             // ✅ ─── Leave ──────────────────────────────────────────────────
//             modelBuilder.Entity<Leave>(entity =>
//             {
//                 entity.HasKey(e => e.LeaveId);

//                 entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
//                 entity.Property(e => e.AdminRemark).HasMaxLength(500);
//                 entity.Property(e => e.TotalDays).HasDefaultValue(1);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── LocationTracking ───────────────────────────────────────
//             modelBuilder.Entity<LocationTracking>(entity =>
//             {
//                 entity.HasKey(e => e.TrackingId);

//                 entity.Property(e => e.CheckInAddress).HasMaxLength(500);
//                 entity.Property(e => e.CheckOutAddress).HasMaxLength(500);
//                 entity.Property(e => e.ClientName).HasMaxLength(200);
//                 entity.Property(e => e.ClientAddress).HasMaxLength(500);
//                 entity.Property(e => e.VisitPurpose).HasMaxLength(500);
//                 entity.Property(e => e.MeetingNotes).HasMaxLength(1000);
//                 entity.Property(e => e.Outcome).HasMaxLength(500);
//                 entity.Property(e => e.WorkType).HasMaxLength(20).HasDefaultValue("office");
//                 entity.Property(e => e.IsClientVisit).HasDefaultValue(false);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── DailyTask ──────────────────────────────────────────────
//             modelBuilder.Entity<DailyTask>(entity =>
//             {
//                 entity.HasKey(e => e.TaskId);

//                 entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(200);
//                 entity.Property(e => e.TaskDescription).HasMaxLength(1000);
//                 entity.Property(e => e.ProjectName).HasMaxLength(200);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("completed");
//                 entity.Property(e => e.HoursSpent).HasColumnType("decimal(5,2)").HasDefaultValue(0);
//                 entity.Property(e => e.Remarks).HasMaxLength(500);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ NEW ─── UserLoginHistory ───────────────────────────────────
//             modelBuilder.Entity<UserLoginHistory>(entity =>
//             {
//                 entity.HasKey(e => e.Id);

//                 entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.DeviceType).HasMaxLength(50);
//                 entity.Property(e => e.DeviceName).HasMaxLength(200);

//                 entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
//                 entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

//                 // Index for faster queries
//                 entity.HasIndex(e => e.UserId);
//                 entity.HasIndex(e => e.LoginDate);

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);

//                 entity.Property(e => e.LogoutReason).HasMaxLength(20); // ✅ ADD

//             });

//             // // ─── Seed: Roles ──────────────────────────────────────────────
//             // modelBuilder.Entity<Role>().HasData(
//             //     new Role
//             //     {
//             //         RoleId         = 1,
//             //         RoleName       = "admin",
//             //         Description    = "System Admin",
//             //         RequiresSelfie = false,
//             //         IsActive       = true,
//             //         CreatedOn      = new DateTime(2026, 2, 14, 0, 0, 0)
//             //     }
//             // );
//         }
//     }
// }












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
//         public DbSet<User>              Users              { get; set; } = null!;
//         public DbSet<Attendance>        Attendances        { get; set; } = null!;
//         public DbSet<Role>              Roles              { get; set; } = null!;
//         public DbSet<Holiday>           Holidays           { get; set; } = null!;
//         public DbSet<WFHRequest>        WFHRequests        { get; set; } = null!;
//         public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

//         // ✅ New DbSets
//         public DbSet<Notification>   Notifications   { get; set; } = null!;
//         public DbSet<Faq>            Faqs            { get; set; } = null!;
//         public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

//         // ✅ New DbSets — Leave, Location, DailyTask
//         public DbSet<Leave>            Leaves            { get; set; } = null!;
//         public DbSet<LocationTracking> LocationTrackings { get; set; } = null!;
//         public DbSet<DailyTask>        DailyTasks        { get; set; } = null!;

//         // ✅ NEW — Login History
//         public DbSet<UserLoginHistory> UserLoginHistories { get; set; } = null!;

//         // ✅ NEW — Asset Management
//         public DbSet<Asset>        Assets         { get; set; } = null!;
//         public DbSet<AssetHistory> AssetHistories { get; set; } = null!;

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

//             // ✅ ─── Leave ──────────────────────────────────────────────────
//             modelBuilder.Entity<Leave>(entity =>
//             {
//                 entity.HasKey(e => e.LeaveId);

//                 entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
//                 entity.Property(e => e.AdminRemark).HasMaxLength(500);
//                 entity.Property(e => e.TotalDays).HasDefaultValue(1);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── LocationTracking ───────────────────────────────────────
//             modelBuilder.Entity<LocationTracking>(entity =>
//             {
//                 entity.HasKey(e => e.TrackingId);

//                 entity.Property(e => e.CheckInAddress).HasMaxLength(500);
//                 entity.Property(e => e.CheckOutAddress).HasMaxLength(500);
//                 entity.Property(e => e.ClientName).HasMaxLength(200);
//                 entity.Property(e => e.ClientAddress).HasMaxLength(500);
//                 entity.Property(e => e.VisitPurpose).HasMaxLength(500);
//                 entity.Property(e => e.MeetingNotes).HasMaxLength(1000);
//                 entity.Property(e => e.Outcome).HasMaxLength(500);
//                 entity.Property(e => e.WorkType).HasMaxLength(20).HasDefaultValue("office");
//                 entity.Property(e => e.IsClientVisit).HasDefaultValue(false);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ ─── DailyTask ──────────────────────────────────────────────
//             modelBuilder.Entity<DailyTask>(entity =>
//             {
//                 entity.HasKey(e => e.TaskId);

//                 entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(200);
//                 entity.Property(e => e.TaskDescription).HasMaxLength(1000);
//                 entity.Property(e => e.ProjectName).HasMaxLength(200);
//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("completed");
//                 entity.Property(e => e.HoursSpent).HasColumnType("decimal(5,2)").HasDefaultValue(0);
//                 entity.Property(e => e.Remarks).HasMaxLength(500);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//             // ✅ NEW ─── UserLoginHistory ───────────────────────────────────
//             modelBuilder.Entity<UserLoginHistory>(entity =>
//             {
//                 entity.HasKey(e => e.Id);

//                 entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.DeviceType).HasMaxLength(50);
//                 entity.Property(e => e.DeviceName).HasMaxLength(200);

//                 entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
//                 entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

//                 entity.HasIndex(e => e.UserId);
//                 entity.HasIndex(e => e.LoginDate);

//                 entity.HasOne(e => e.User)
//                     .WithMany()
//                     .HasForeignKey(e => e.UserId)
//                     .OnDelete(DeleteBehavior.Cascade);

//                 entity.Property(e => e.LogoutReason).HasMaxLength(20);
//             });

//             // ✅ NEW ─── Asset ──────────────────────────────────────────────
//             modelBuilder.Entity<Asset>(entity =>
//             {
//                 entity.HasKey(e => e.AssetId);

//                 entity.HasIndex(e => e.AssetCode)
//                       .IsUnique()
//                       .HasFilter("[AssetCode] IS NOT NULL");

//                 entity.Property(e => e.AssetName).IsRequired().HasMaxLength(100);
//                 entity.Property(e => e.AssetType).IsRequired().HasMaxLength(50);
//                 entity.Property(e => e.AssetCode).HasMaxLength(100);
//                 entity.Property(e => e.SerialNumber).HasMaxLength(100);
//                 entity.Property(e => e.Brand).HasMaxLength(100);
//                 entity.Property(e => e.Model).HasMaxLength(100);
//                 entity.Property(e => e.Description).HasMaxLength(500);

//                 entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("available");
//                 entity.Property(e => e.AssignmentNote).HasMaxLength(500);
//                 entity.Property(e => e.ReturnNote).HasMaxLength(500);
//                 entity.Property(e => e.ReturnCondition).HasMaxLength(20);

//                 entity.Property(e => e.IsActive).HasDefaultValue(true);
//                 entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.AssignedToUser)
//                       .WithMany()
//                       .HasForeignKey(e => e.AssignedToUserId)
//                       .OnDelete(DeleteBehavior.SetNull);

//                 entity.HasOne(e => e.CreatedByUser)
//                       .WithMany()
//                       .HasForeignKey(e => e.CreatedByUserId)
//                       .OnDelete(DeleteBehavior.NoAction);
//             });

//             // ✅ NEW ─── AssetHistory ───────────────────────────────────────
//             modelBuilder.Entity<AssetHistory>(entity =>
//             {
//                 entity.HasKey(e => e.HistoryId);

//                 entity.HasIndex(e => e.AssetId);
//                 entity.HasIndex(e => e.UserId);

//                 entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
//                 entity.Property(e => e.Note).HasMaxLength(500);
//                 entity.Property(e => e.Condition).HasMaxLength(20);
//                 entity.Property(e => e.ActionDate).HasDefaultValueSql("GETDATE()");

//                 entity.HasOne(e => e.Asset)
//                       .WithMany(a => a.Histories)
//                       .HasForeignKey(e => e.AssetId)
//                       .OnDelete(DeleteBehavior.Cascade);

//                 entity.HasOne(e => e.User)
//                       .WithMany()
//                       .HasForeignKey(e => e.UserId)
//                       .OnDelete(DeleteBehavior.NoAction);
//             });
//         }
//     }
// }









// ======================= Data/ApplicationDbContext.cs =======================
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
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Faq> Faqs { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<Leave> Leaves { get; set; } = null!;
        public DbSet<LocationTracking> LocationTrackings { get; set; } = null!;
        public DbSet<DailyTask> DailyTasks { get; set; } = null!;
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<AssetHistory> AssetHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Attendance decimal precision
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.Property(e => e.InLatitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.InLongitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.OutLatitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.OutLongitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.TotalHours).HasColumnType("decimal(6,2)");
            });

            // ✅ DailyTask decimal precision
            modelBuilder.Entity<DailyTask>(entity =>
            {
                entity.Property(e => e.HoursSpent).HasColumnType("decimal(6,2)");
            });

            // ✅ Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.AssetId);

                // Fix #11: index covers ALL rows (active + inactive)
                // Pehle sirf IsActive=true rows check hoti thi, isse inactive
                // asset ka code reuse ho sakta tha aur reactivate karne par DB crash.
                entity.HasIndex(e => e.AssetCode)
                      .IsUnique()
                      .HasFilter("[AssetCode] IS NOT NULL");  // NULL allowed, non-null must be unique

                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AssetType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AssetCode).HasMaxLength(100);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("available");
                entity.Property(e => e.AssignmentNote).HasMaxLength(500);
                entity.Property(e => e.ReturnNote).HasMaxLength(500);
                entity.Property(e => e.ReturnCondition).HasMaxLength(20);

                entity.Property(e => e.MaintenanceType).HasMaxLength(30);
                entity.Property(e => e.MaintenanceVendorName).HasMaxLength(100);
                entity.Property(e => e.MaintenanceTicketNo).HasMaxLength(100);
                entity.Property(e => e.MaintenanceIssue).HasMaxLength(500);
                entity.Property(e => e.MaintenanceCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaintenanceResolution).HasMaxLength(500);

                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Fix #6: GETUTCDATE() instead of GETDATE()
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ✅ AssetHistory
            modelBuilder.Entity<AssetHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryId);

                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);

                // Fix #4: 2000 chars for JSON maintenance snapshots
                entity.Property(e => e.Note).HasMaxLength(2000);

                entity.Property(e => e.Condition).HasMaxLength(20);

                // Fix #6: GETUTCDATE() instead of GETDATE()
                entity.Property(e => e.ActionDate).HasDefaultValueSql("GETUTCDATE()");

                // Fix #10: Cascade → Restrict
                // Asset hard-delete karne par history silently delete na ho.
                // Asset ko IsActive=false karke soft-delete karo.
                // Agar koi hard-delete try kare toh DB error dega — safe!
                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.Histories)
                      .HasForeignKey(e => e.AssetId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}