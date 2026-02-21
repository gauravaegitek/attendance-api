using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    // ─── Get Profile Response ─────────────────────────────────────────
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string JoiningDate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? LastSeen { get; set; }
    }

    // ─── Update Profile Request ───────────────────────────────────────
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        public IFormFile? ProfilePhoto { get; set; }
    }

    // ─── Change Password ──────────────────────────────────────────────
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}